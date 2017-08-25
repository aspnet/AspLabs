// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> to allow only WebHook requests with a <c>Content-Type</c> matching
    /// <see cref="IWebHookRequestMetadata.BodyType"/>.
    /// </summary>
    /// <remarks>
    /// Done as an <see cref="IResourceFilter"/> implementation and not an
    /// <see cref="Mvc.ActionConstraints.IActionConstraintMetadata"/> because receivers do not dynamically vary their
    /// <see cref="IWebHookRequestMetadata"/>. Use distinct <see cref="WebHookAttribute.Id"/> values if different
    /// configurations are needed for one receiver.
    /// </remarks>
    public class WebHookVerifyBodyTypeFilter : IResourceFilter, IOrderedFilter
    {
        private readonly ILogger _logger;
        private readonly IWebHookRequestMetadata _requestMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyMethodFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="requestMetadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        public WebHookVerifyBodyTypeFilter(ILoggerFactory loggerFactory, IWebHookRequestMetadata requestMetadata)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (requestMetadata == null)
            {
                throw new ArgumentNullException(nameof(requestMetadata));
            }

            _logger = loggerFactory.CreateLogger<WebHookVerifyBodyTypeFilter>();
            _requestMetadata = requestMetadata;
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> used in all <see cref="WebHookVerifyBodyTypeFilter"/>
        /// instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm signature or <c>code</c> query parameter (e.g. in <see cref="WebHookVerifyCodeFilter"/> or a
        /// <see cref="WebHookVerifyBodyContentFilter"/> subclass).
        /// </item>
        /// <item>
        /// Confirm required headers and query parameters are provided (in
        /// <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in
        /// <see cref="WebHookGetResponseFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in this filter).</item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetResponseFilter"/> for this receiver (in
        /// <see cref="WebHookPingResponseFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookVerifyMethodFilter.Order + 10;

        /// <inheritdoc />
        int IOrderedFilter.Order => Order;

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var request = context.HttpContext.Request;
            switch (_requestMetadata.BodyType)
            {
                case WebHookBodyType.Form:
                    if (!request.HasFormContentType)
                    {
                        context.Result = CreateUnsupportedMediaTypeResult(Resources.VerifyBody_NoFormData);
                    }
                    break;

                case WebHookBodyType.Json:
                    if (!request.IsJson())
                    {
                        context.Result = CreateUnsupportedMediaTypeResult(Resources.VerifyBody_NoJson);
                    }
                    break;

                case WebHookBodyType.Xml:
                    if (!request.IsXml())
                    {
                        context.Result = CreateUnsupportedMediaTypeResult(Resources.VerifyBody_NoXml);
                    }
                    break;
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        private IActionResult CreateUnsupportedMediaTypeResult(string message)
        {
            _logger.LogInformation(0, message);

            // ??? Should we instead provide CreateErrorResult(...) overloads with `int statusCode` parameters?
            var badMethod = WebHookResultUtilities.CreateErrorResult(message);
            badMethod.StatusCode = StatusCodes.Status415UnsupportedMediaType;

            return badMethod;
        }
    }
}
