// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IAsyncResultFilter"/> implementation to add an acknowledgment body to successful results.
    /// </summary>
    public class SalesforceAcknowledgmentFilter : IAsyncResultFilter, IWebHookReceiver
    {
        private readonly ISalesforceResultCreator _resultCreator;

        /// <summary>
        /// Instantiates a new <see cref="SalesforceAcknowledgmentFilter"/> instance.
        /// </summary>
        /// <param name="resultCreator">The <see cref="ISalesforceResultCreator"/>.</param>
        public SalesforceAcknowledgmentFilter(ISalesforceResultCreator resultCreator)
        {
            if (resultCreator == null)
            {
                throw new ArgumentNullException(nameof(resultCreator));
            }

            _resultCreator = resultCreator;
        }

        /// <inheritdoc />
        public string ReceiverName => SalesforceConstants.ReceiverName;

        /// <inheritdoc />
        public bool IsApplicable(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            return string.Equals(ReceiverName, receiverName, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        /// <inheritdoc />
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (!context.RouteData.TryGetWebHookReceiverName(out var receiverName) || !IsApplicable(receiverName))
            {
                await next();
                return;
            }

            // ??? Are these conditions too fragile? May not handle new subclasses of the result types.
            // ??? Should these cases short-circuit other result filters?
            if (context.Result == null || context.Result is EmptyResult)
            {
                context.Result = await _resultCreator.GetSuccessResultAsync();
            }
            else if (context.Result is ContentResult contentResult &&
                    string.IsNullOrEmpty(contentResult.Content) &&
                    InRangeStatusCode(contentResult.StatusCode))
            {
                var newResult = await _resultCreator.GetSuccessResultAsync();
                if (contentResult.StatusCode.HasValue)
                {
                    newResult.StatusCode = newResult.StatusCode;
                }

                context.Result = newResult;
            }
            else if (context.Result is StatusCodeResult statusCodeResult &&
                    InRangeStatusCode(statusCodeResult.StatusCode))
            {
                var newResult = await _resultCreator.GetSuccessResultAsync();
                newResult.StatusCode = statusCodeResult.StatusCode;
                context.Result = newResult;
            }

            await next();
        }

        private bool InRangeStatusCode(int? statusCode)
        {
            if (!statusCode.HasValue)
            {
                // Default status code is in hoped-for range.
                return true;
            }

            return InRangeStatusCode(statusCode.Value);
        }

        private bool InRangeStatusCode(int statusCode)
        {
            return statusCode >= StatusCodes.Status200OK && statusCode < StatusCodes.Status300MultipleChoices;
        }
    }
}
