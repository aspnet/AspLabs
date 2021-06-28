// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    public class WebHookPingRequestFilter : IResourceFilter, IOrderedFilter
    {
        private readonly ILogger _logger;
        private readonly WebHookMetadataProvider _metadataProvider;
        private readonly IWebHookPingRequestMetadata _pingRequestMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookPingRequestFilter"/> instance to short-circuit WebHook requests based
        /// on given <paramref name="pingRequestMetadata"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="pingRequestMetadata">
        /// The receiver's <see cref="IWebHookPingRequestMetadata"/>.
        /// </param>
        public WebHookPingRequestFilter(
            ILoggerFactory loggerFactory,
            IWebHookPingRequestMetadata pingRequestMetadata)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<WebHookPingRequestFilter>();
            _pingRequestMetadata = pingRequestMetadata ?? throw new ArgumentNullException(nameof(pingRequestMetadata));
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookPingRequestFilter"/> instance to short-circuit WebHook requests based
        /// on the receiver's <see cref="IWebHookPingRequestMetadata"/>. That metadata is found in
        /// <paramref name="metadataProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadataProvider">
        /// The <see cref="WebHookMetadataProvider"/> service. Searched for applicable metadata per-request.
        /// </param>
        /// <remarks>This overload is intended for use with <see cref="GeneralWebHookAttribute"/>.</remarks>
        public WebHookPingRequestFilter(
            ILoggerFactory loggerFactory,
            WebHookMetadataProvider metadataProvider)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<WebHookPingRequestFilter>();
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookPingRequestFilter"/>
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
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventNameMapperConstraint"/> for this receiver
        /// (in <see cref="WebHookEventNameMapperFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetHeadRequestFilter"/> for this receiver (in
        /// this filter).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookEventNameMapperFilter.Order + 10;

        /// <inheritdoc />
        int IOrderedFilter.Order => Order;

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            var pingRequestMetadata = _pingRequestMetadata;
            if (pingRequestMetadata == null)
            {
                if (!routeData.TryGetWebHookReceiverName(out var requestReceiverName))
                {
                    return;
                }

                pingRequestMetadata = _metadataProvider.GetPingRequestMetadata(requestReceiverName);
                if (pingRequestMetadata == null)
                {
                    return;
                }
            }

            // If this is a ping request, short-circuit further processing.
            if (pingRequestMetadata != null &&
                routeData.TryGetWebHookEventName(out var eventName) &&
                string.Equals(eventName, pingRequestMetadata.PingEventName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    0,
                    "Received a Ping Event for the '{ReceiverName}' WebHook receiver -- ignoring.",
                    pingRequestMetadata.ReceiverName);

                context.Result = new OkResult();
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }
    }
}
