// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> to allow only POST WebHook requests with a non-empty request body. To support
    /// GET or HEAD requests the receiver project should implement
    /// <see cref="Metadata.IWebHookGetHeadRequestMetadata"/> in its metadata service.
    /// </summary>
    /// <remarks>
    /// Done as an <see cref="IResourceFilter"/> implementation and not an
    /// <see cref="Mvc.ActionConstraints.IActionConstraintMetadata"/> because GET and HEAD requests (often pings or
    /// simple verifications) are never of interest in user code. However, some senders require specific responses to
    /// GET or HEAD requests and those requests and responses are handled in <see cref="WebHookGetHeadRequestFilter"/>.
    /// </remarks>
    public class WebHookVerifyMethodFilter : IResourceFilter, IOrderedFilter
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyMethodFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookVerifyMethodFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebHookVerifyMethodFilter>();
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookVerifyMethodFilter"/>
        /// instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm WebHooks configuration is set up correctly (in <see cref="WebHookReceiverExistsFilter"/>).
        /// </item>
        /// <item>
        /// Confirm signature or <c>code</c> query parameter e.g. in <see cref="WebHookVerifyCodeFilter"/> or other
        /// <see cref="WebHookSecurityFilter"/> subclass.
        /// </item>
        /// <item>
        /// Confirm required headers, <see cref="RouteValueDictionary"/> entries and query parameters are provided (in
        /// <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in
        /// <see cref="WebHookGetHeadRequestFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in this filter).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventNameMapperConstraint"/> for this receiver
        /// (in <see cref="WebHookEventNameMapperFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetHeadRequestFilter"/> for this receiver (in
        /// <see cref="WebHookPingRequestFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookGetHeadRequestFilter.Order + 10;

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
            if (request.Body == null ||
                !request.ContentLength.HasValue ||
                request.ContentLength.Value == 0L ||
                !HttpMethods.IsPost(request.Method))
            {
                // Log about the issue and short-circuit remainder of the pipeline.
                context.RouteData.TryGetWebHookReceiverName(out var receiverName);
                context.Result = CreateBadMethodResult(request.Method, receiverName);
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        private IActionResult CreateBadMethodResult(string methodName, string receiverName)
        {
            _logger.LogWarning(
                0,
                "The HTTP '{RequestMethod}' method is not supported by the '{ReceiverName}' WebHook receiver.",
                methodName,
                receiverName);

            var message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.VerifyMethod_BadMethod,
                methodName,
                receiverName);
            var badMethod = new BadRequestObjectResult(message)
            {
                StatusCode = StatusCodes.Status405MethodNotAllowed
            };

            return badMethod;
        }
    }
}
