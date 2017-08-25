// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> that verifies the <c>code</c> query parameter. Short-circuits the request if
    /// the <c>code</c> query parameter is missing or does not match the receiver's configuration. Also confirms the
    /// request URI uses the <c>HTTPS</c> scheme.
    /// </summary>
    public class WebHookVerifyCodeFilter : WebHookSecurityFilter, IAsyncResourceFilter
    {
        private readonly IReadOnlyList<IWebHookSecurityMetadata> _codeVerifierMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyCodeFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to initialize <see cref="WebHookSecurityFilter.Logger"/>.
        /// </param>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        /// <param name="receiverConfig">
        /// The <see cref="IWebHookReceiverConfig"/> used to initialize
        /// <see cref="WebHookSecurityFilter.Configuration"/> and <see cref="WebHookSecurityFilter.ReceiverConfig"/>.
        /// </param>
        public WebHookVerifyCodeFilter(
            ILoggerFactory loggerFactory,
            IEnumerable<IWebHookMetadata> metadata,
            IWebHookReceiverConfig receiverConfig)
            : base(loggerFactory, receiverConfig)
        {
            // No need to keep track of IWebHookSecurityMetadata instances that do not request code verification.
            var codeVerifierMetadata = metadata
                .OfType<IWebHookSecurityMetadata>()
                .Where(item => item.VerifyCodeParameter);
            _codeVerifierMetadata = new List<IWebHookSecurityMetadata>(codeVerifierMetadata);
        }

        /// <inheritdoc />
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var routeData = context.RouteData;
            if (routeData.TryGetReceiverName(out var receiverName) &&
                _codeVerifierMetadata.Any(metadata => metadata.IsApplicable(receiverName)))
            {
                var result = await EnsureValidCode(context.HttpContext.Request, routeData, receiverName);
                if (result != null)
                {
                    context.Result = result;
                    return;
                }
            }

            await next();
        }

        /// <summary>
        /// For WebHook providers with insufficient security considerations, the receiver can require that the WebHook
        /// URI must be an <c>https</c> URI and contain a 'code' query parameter with a value configured for that
        /// particular <c>id</c>. A sample WebHook URI is
        /// '<c>https://&lt;host&gt;/api/webhooks/incoming/&lt;receiver&gt;?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
        /// The 'code' parameter must be between 32 and 128 characters long.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="routeData">
        /// The <see cref="RouteData"/> for this request. A (potentially empty) ID value in this data allows a
        /// <see cref="WebHookVerifyCodeFilter"/> to support multiple senders with individual configurations.
        /// </param>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides <see langword="null"/> in the success case. When a check
        /// fails, provides an <see cref="IActionResult"/> that when executed will produce a response containing
        /// details about the problem.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed by Web API.")]
        protected virtual async Task<IActionResult> EnsureValidCode(
            HttpRequest request,
            RouteData routeData,
            string receiverName)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            var result = EnsureSecureConnection(receiverName, request);
            if (result != null)
            {
                return result;
            }

            var code = request.Query[WebHookConstants.CodeQueryParameterName];
            if (StringValues.IsNullOrEmpty(code))
            {
                Logger.LogError(
                    400,
                    "The WebHook verification request must contain a '{ParameterName}' query parameter.",
                    WebHookConstants.CodeQueryParameterName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.General_MissingQueryParameter,
                    WebHookConstants.CodeQueryParameterName);
                var noCode = WebHookResultUtilities.CreateErrorResult(message);

                return noCode;
            }

            var secretKey = await GetReceiverConfig(
                request,
                routeData,
                receiverName,
                WebHookConstants.CodeParameterMinLength,
                WebHookConstants.CodeParameterMaxLength);
            if (secretKey == null)
            {
                return new NotFoundResult();
            }

            if (!SecretEqual(code, secretKey))
            {
                Logger.LogError(
                    401,
                    "The '{ParameterName}' query parameter provided in the HTTP request did not match the " +
                    "expected value.",
                    WebHookConstants.CodeQueryParameterName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifyCode_BadCode,
                    WebHookConstants.CodeQueryParameterName);
                var invalidCode = WebHookResultUtilities.CreateErrorResult(message);

                return invalidCode;
            }

            return null;
        }
    }
}
