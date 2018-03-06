// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> that verifies the <c>code</c> query parameter. Short-circuits the request if
    /// the <c>code</c> query parameter is missing or does not match the receiver's configuration. Also confirms the
    /// request URI uses the <c>HTTPS</c> scheme.
    /// </summary>
    public class WebHookVerifyCodeFilter : WebHookSecurityFilter, IResourceFilter
    {
        private readonly IReadOnlyList<IWebHookVerifyCodeMetadata> _verifyCodeMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyCodeFilter"/> instance.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> used to initialize <see cref="WebHookSecurityFilter.Configuration"/>.
        /// </param>
        /// <param name="hostingEnvironment">
        /// The <see cref="IHostingEnvironment" /> used to initialize
        /// <see cref="WebHookSecurityFilter.HostingEnvironment"/>.
        /// </param>
        /// <param name="verifyCodeMetadata">
        /// The collection of <see cref="IWebHookVerifyCodeMetadata"/> services.
        /// </param>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to initialize <see cref="WebHookSecurityFilter.Logger"/>.
        /// </param>
        public WebHookVerifyCodeFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            IEnumerable<IWebHookVerifyCodeMetadata> verifyCodeMetadata,
            ILoggerFactory loggerFactory)
            : base(configuration, hostingEnvironment, loggerFactory)
        {
            _verifyCodeMetadata = verifyCodeMetadata.ToArray();
        }

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            if (routeData.TryGetWebHookReceiverName(out var receiverName) &&
                _verifyCodeMetadata.Any(metadata => metadata.IsApplicable(receiverName)))
            {
                var result = EnsureValidCode(context.HttpContext.Request, routeData, receiverName);
                if (result != null)
                {
                    context.Result = result;
                }
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        /// <summary>
        /// For WebHook providers with insufficient security considerations, the receiver can require that the WebHook
        /// URI must be an <c>https</c> URI and contain a 'code' query parameter with a value configured for that
        /// particular <c>id</c>. A sample WebHook URI is
        /// '<c>https://{host}/api/webhooks/incoming/{receiver name}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
        /// The 'code' parameter must be between 32 and 128 characters long.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="routeData">
        /// The <see cref="RouteData"/> for this request. A (potentially empty) ID value in this data allows a
        /// <see cref="WebHookVerifyCodeFilter"/> to support multiple senders with individual configurations.
        /// </param>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// <see langword="null"/> in the success case. When a check fails, an <see cref="IActionResult"/> that when
        /// executed will produce a response containing details about the problem.
        /// </returns>
        protected virtual IActionResult EnsureValidCode(
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
                Logger.LogWarning(
                    400,
                    "A '{ReceiverName}' WebHook verification request must contain a " +
                    $"'{WebHookConstants.CodeQueryParameterName}' query " +
                    "parameter.",
                    receiverName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.General_NoQueryParameter,
                    receiverName,
                    WebHookConstants.CodeQueryParameterName);
                var noCode = new BadRequestObjectResult(message);

                return noCode;
            }

            var secretKey = GetSecretKey(
                receiverName,
                routeData,
                WebHookConstants.CodeParameterMinLength,
                WebHookConstants.CodeParameterMaxLength);
            if (secretKey == null)
            {
                return new NotFoundResult();
            }

            if (!SecretEqual(code, secretKey))
            {
                Logger.LogWarning(
                    401,
                    $"The '{WebHookConstants.CodeQueryParameterName}' query parameter provided in the HTTP request " +
                    "did not match the expected value.");

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifyCode_BadCode,
                    WebHookConstants.CodeQueryParameterName);
                var invalidCode = new BadRequestObjectResult(message);

                return invalidCode;
            }

            return null;
        }
    }
}
