// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> to short-circuit ping WebHook requests.
    /// </summary>
    public class WebHookPingRequestFilter : IResourceFilter
    {
        private readonly ILogger _logger;
        private readonly IReadOnlyList<IWebHookPingRequestMetadata> _pingMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookPingRequestFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        public WebHookPingRequestFilter(ILoggerFactory loggerFactory, IEnumerable<IWebHookMetadata> metadata)
        {
            _logger = loggerFactory.CreateLogger<WebHookPingRequestFilter>();
            _pingMetadata = metadata.OfType<IWebHookPingRequestMetadata>().ToArray();
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookPingRequestFilter"/>
        /// instances. The recommended filter sequence is
        /// <list type="number">
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
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventMapperConstraint"/> for this receiver (in
        /// <see cref="WebHookEventMapperFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetHeadRequestFilter"/> for this receiver (in
        /// this filter).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookVerifyBodyTypeFilter.Order + 10;

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            if (routeData.TryGetWebHookReceiverName(out var receiverName))
            {
                var pingMetadata = _pingMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (pingMetadata != null &&
                    routeData.TryGetWebHookEventName(out var eventName))
                {
                    // If this is a ping request, short-circuit further processing.
                    if (string.Equals(eventName, pingMetadata.PingEventName, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation(
                            0,
                            "Received a Ping Event for the '{ReceiverName}' WebHook receiver -- ignoring.",
                            receiverName);

                        context.Result = new OkResult();
                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }
    }
}
