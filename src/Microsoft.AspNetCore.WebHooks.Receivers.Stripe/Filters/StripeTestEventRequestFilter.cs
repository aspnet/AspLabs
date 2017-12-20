// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> that logs about and optionally short-circuits Stripe test events. Does not
    /// short-circuit test events when the <c>WebHooks:Stripe:PassThroughTestEvents</c> configuration value is
    /// <c>true</c>.
    /// </summary>
    /// <remarks>Somewhat similar to the <see cref="WebHookPingRequestFilter"/>.</remarks>
    public class StripeTestEventRequestFilter : IResourceFilter, IWebHookReceiver
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="StripeTestEventRequestFilter"/> instance.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public StripeTestEventRequestFilter(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<StripeTestEventRequestFilter>();
        }

        /// <inheritdoc />
        public string ReceiverName => StripeConstants.ReceiverName;

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="StripeTestEventRequestFilter"/>
        /// instances. This filter should execute in the same slot as <see cref="WebHookPingRequestFilter"/>.
        /// <see cref="WebHookPingRequestFilter"/> does not apply for this receiver.
        /// </summary>
        public static int Order => WebHookPingRequestFilter.Order;

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
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            if (!routeData.TryGetWebHookReceiverName(out var receiverName) || !IsApplicable(receiverName))
            {
                return;
            }

            var notificationId = (string)routeData.Values[StripeConstants.NotificationIdKeyName];
            if (IsTestEvent(notificationId))
            {
                // Log about and optionally short-circuit this test event.
                var passThroughString = _configuration[StripeConstants.PassThroughTestEventsConfigurationKey];
                if (bool.TryParse(passThroughString, out var passThrough) && passThrough)
                {
                    _logger.LogInformation(0, "Received a Stripe Test Event.");
                }
                else
                {
                    _logger.LogInformation(1, "Ignoring a Stripe Test Event.");
                    context.Result = new OkResult();
                }
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        internal static bool IsTestEvent(string notificationId)
        {
            return string.Equals(
                StripeConstants.TestNotificationId,
                notificationId,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
