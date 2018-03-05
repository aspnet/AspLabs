// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> implementation which uses <see cref="IWebHookEventMetadata"/> to determine
    /// the event name(s) for a WebHook request. This constraint accepts all candidates for receivers lacking
    /// <see cref="IWebHookEventMetadata"/> or with <see cref="IWebHookEventMetadata.ConstantValue"/>
    /// non-<see langword="null"/>. Otherwise, the request must contain one or more event names.
    /// </summary>
    public class WebHookEventMapperConstraint : IActionConstraint
    {
        private readonly IReadOnlyList<IWebHookEventMetadata> _eventMetadata;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventMapperConstraint"/> instance with the given
        /// <paramref name="loggerFactory"/> and <paramref name="metadata"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        public WebHookEventMapperConstraint(ILoggerFactory loggerFactory, IEnumerable<IWebHookMetadata> metadata)
        {
            _eventMetadata = metadata.OfType<IWebHookEventMetadata>().ToArray();
            _logger = loggerFactory.CreateLogger<WebHookEventMapperConstraint>();
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
        public bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeContext = context.RouteContext;
            if (routeContext.RouteData.TryGetWebHookReceiverName(out var receiverName))
            {
                var eventMetadata = _eventMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (eventMetadata != null)
                {
                    return Accept(eventMetadata, routeContext);
                }

                // This receiver does not have IWebHookEventMetadata and that's fine.
                return true;
            }

            var message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.EventConstraints_NoReceiverName,
                typeof(WebHookReceiverExistsConstraint));
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Gets an indication whether the expected event names are available in the request. Maps the event names into
        /// route values.
        /// </summary>
        /// <param name="eventMetadata">The <see cref="IWebHookEventMetadata"/> for this receiver.</param>
        /// <param name="routeContext">The <see cref="RouteContext"/> for this constraint and request.</param>
        /// <returns>
        /// <see langword="true"/> if event names are available in the request; <see langword="false"/> otherwise.
        /// </returns>
        protected virtual bool Accept(IWebHookEventMetadata eventMetadata, RouteContext routeContext)
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
            if (eventMetadata.HeaderName != null)
            {
                var headers = request.Headers;

                var events = headers.GetCommaSeparatedValues(eventMetadata.HeaderName);
                if (events.Length > 0)
                {
                    routeData.SetWebHookEventNames(events);
                    return true;
                }
            }

            if (eventMetadata.QueryParameterName != null)
            {
                var query = request.Query;
                if (query.TryGetValue(eventMetadata.QueryParameterName, out var events) &&
                    events.Count > 0)
                {
                    routeData.SetWebHookEventNames(events);
                    return true;
                }
            }

            if (eventMetadata.ConstantValue != null)
            {
                routeData.Values[WebHookConstants.EventKeyName] = eventMetadata.ConstantValue;
                return true;
            }

            routeData.TryGetWebHookReceiverName(out var receiverName);
            if (eventMetadata.QueryParameterName == null)
            {
                _logger.LogWarning(
                    0,
                    "A '{ReceiverName}' WebHook request must contain a '{HeaderName}' HTTP header " +
                    "indicating the type of event.",
                    receiverName,
                    eventMetadata.HeaderName);
            }
            else if (eventMetadata.HeaderName == null)
            {
                _logger.LogWarning(
                    1,
                    "A '{ReceiverName}' WebHook request must contain a '{QueryParameterKey}' query " +
                    "parameter indicating the type of event.",
                    receiverName,
                    eventMetadata.QueryParameterName);
            }
            else
            {
                _logger.LogWarning(
                    2,
                    "A '{ReceiverName}' WebHook request must contain a '{HeaderName}' HTTP header or a " +
                    "'{QueryParameterKey}' query parameter indicating the type of event.",
                    receiverName,
                    eventMetadata.HeaderName,
                    eventMetadata.QueryParameterName);
            }

            return false;
        }
    }
}
