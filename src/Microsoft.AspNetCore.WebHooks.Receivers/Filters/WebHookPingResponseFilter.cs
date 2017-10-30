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
    public class WebHookPingResponseFilter : IResourceFilter
    {
        private readonly ILogger _logger;
        private readonly IReadOnlyList<IWebHookEventMetadata> _eventMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookPingResponseFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        public WebHookPingResponseFilter(ILoggerFactory loggerFactory, IEnumerable<IWebHookMetadata> metadata)
        {
            _logger = loggerFactory.CreateLogger<WebHookPingResponseFilter>();
            _eventMetadata = metadata.OfType<IWebHookEventMetadata>().ToArray();
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookPingResponseFilter"/>
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
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetResponseFilter"/> for this receiver (in
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
            if (routeData.TryGetWebHookEventName(out var eventName) &&
                routeData.TryGetWebHookReceiverName(out var receiverName))
            {
                var eventMetadata = _eventMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                var pingEventName = eventMetadata?.PingEventName;

                // If this is a ping request, short-circuit further processing.
                if (pingEventName != null &&
                    string.Equals(eventName, pingEventName, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation(0, "Received a {ReceiverName} Ping Event -- ignoring.", receiverName);

                    context.Result = new OkResult();
                    return;
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
