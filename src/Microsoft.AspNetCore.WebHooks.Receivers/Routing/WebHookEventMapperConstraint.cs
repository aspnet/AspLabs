// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An base class for <see cref="IActionConstraint"/> implementations which use
    /// <see cref="IWebHookEventMetadata"/> to determine the event name for a WebHook request. This constraint
    /// almost-always accepts all candidates.
    /// </summary>
    public abstract class WebHookEventMapperConstraint : IActionConstraint
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventMapperConstraint"/> instance with the given
        /// <paramref name="loggerFactory"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        protected WebHookEventMapperConstraint(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Gets the <see cref="IActionConstraint.Order"/> value used in all <see cref="WebHookEventMapperConstraint"/>
        /// instances.
        /// </summary>
        /// <value>Chosen to run this constraint just after <see cref="WebHookIdConstraint"/>.</value>
        public static int Order => WebHookIdConstraint.Order + 10;

        /// <inheritdoc />
        int IActionConstraint.Order => Order;

        /// <inheritdoc />
        public abstract bool Accept(ActionConstraintContext context);

        /// <summary>
        /// Gets an indication whether the expected event names are available in the request. Maps the event names into
        /// route values.
        /// </summary>
        /// <param name="eventMetadata">The <see cref="IWebHookEventMetadata"/> for this receiver.</param>
        /// <param name="routeContext">The <see cref="RouteContext"/> for this constraint and request.</param>
        /// <returns>
        /// <see langword="true"/> if event names are available in the request; <see langword="false"/> otherwise.
        /// </returns>
        protected bool Accept(IWebHookEventMetadata eventMetadata, RouteContext routeContext)
        {
            if (eventMetadata == null)
            {
                throw new ArgumentNullException(nameof(eventMetadata));
            }

            if (routeContext == null)
            {
                throw new ArgumentNullException(nameof(routeContext));
            }

            var request = routeContext.HttpContext.Request;
            var routeData = routeContext.RouteData;
            var routeValues = routeData.Values;
            if (eventMetadata.HeaderName != null)
            {
                var headers = request.Headers;

                // ??? Is GetCommaSeparatedValues() overkill?
                var events = headers.GetCommaSeparatedValues(eventMetadata.HeaderName);
                if (events.Length == 0)
                {
                    if (eventMetadata.ConstantValue == null && eventMetadata.QueryParameterName != null)
                    {
                        // An error because we have no fallback.
                        routeData.TryGetReceiverName(out var receiverName);
                        _logger.LogError(
                            500,
                            "A {ReceiverName} WebHook request must contain a '{HeaderName}' HTTP header indicating the type of event.",
                            receiverName,
                            eventMetadata.HeaderName);
                    }
                }
                else
                {
                    MapEventNames(routeValues, events);
                    return true;
                }
            }

            if (eventMetadata.QueryParameterName != null)
            {
                var query = request.Query;
                if (!query.TryGetValue(eventMetadata.QueryParameterName, out var events) ||
                    events.Count == 0)
                {
                    if (eventMetadata.ConstantValue == null)
                    {
                        // An error because we have no fallback.
                        routeData.TryGetReceiverName(out var receiverName);
                        _logger.LogError(
                            501,
                            "A {ReceiverName} WebHook request must contain a '{QueryParameterKey}' query parameter indicating the type of event.",
                            receiverName,
                            eventMetadata.QueryParameterName);
                    }
                }
                else
                {
                    MapEventNames(routeValues, events);
                    return true;
                }
            }

            if (eventMetadata.ConstantValue != null)
            {
                routeValues[WebHookConstants.EventKeyName] = eventMetadata.ConstantValue;
                return true;
            }

            return false;
        }

        private void MapEventNames(RouteValueDictionary routeValues, string[] events)
        {
            if (events.Length == 1)
            {
                routeValues[WebHookConstants.EventKeyName] = events[0];
            }
            else
            {
                // ??? This repeatedly allocates the same strings. Might be good to cache the first 100 or so keys.
                for (var i = 0; i < events.Length; i++)
                {
                    routeValues[$"{WebHookConstants.EventKeyName}[{i}]"] = events[i];
                }
            }
        }
    }
}
