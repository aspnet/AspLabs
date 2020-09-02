// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
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
    public class WebHookEventNameMapperConstraint : IActionConstraint
    {
        private readonly IWebHookEventMetadata _eventMetadata;
        private readonly ILogger _logger;
        private readonly WebHookMetadataProvider _metadataProvider;

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNameMapperConstraint"/> instance to verify the given
        /// <paramref name="eventMetadata"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="eventMetadata">The receiver's <see cref="IWebHookEventMetadata"/>.</param>
        public WebHookEventNameMapperConstraint(
            ILoggerFactory loggerFactory,
            IWebHookEventMetadata eventMetadata)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _eventMetadata = eventMetadata ?? throw new ArgumentNullException(nameof(eventMetadata));
            _logger = loggerFactory.CreateLogger<WebHookEventNameMapperConstraint>();
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNameMapperConstraint"/> instance to verify the receiver's
        /// <see cref="IWebHookEventMetadata"/>. That metadata is found in <paramref name="metadataProvider"/>).
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadataProvider">
        /// The <see cref="WebHookMetadataProvider"/> service. Searched for applicable metadata per-request.
        /// </param>
        /// <remarks>This overload is intended for use with <see cref="GeneralWebHookAttribute"/>.</remarks>
        public WebHookEventNameMapperConstraint(
            ILoggerFactory loggerFactory,
            WebHookMetadataProvider metadataProvider)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<WebHookEventNameMapperConstraint>();
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
        }

        /// <summary>
        /// Gets the <see cref="IActionConstraint.Order"/> value used in all
        /// <see cref="WebHookEventNameMapperConstraint"/>
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
            var eventMetadata = _eventMetadata;
            if (eventMetadata == null)
            {
                if (!routeContext.RouteData.TryGetWebHookReceiverName(
                    context.CurrentCandidate.Action,
                    out var receiverName))
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.EventConstraints_NoReceiverName,
                        typeof(WebHookReceiverNameConstraint));
                    throw new InvalidOperationException(message);
                }

                eventMetadata = _metadataProvider.GetEventMetadata(receiverName);
            }

            if (eventMetadata != null)
            {
                return Accept(eventMetadata, routeContext);
            }

            // This receiver does not have IWebHookEventMetadata and that's fine.
            return true;
        }

        private bool Accept(IWebHookEventMetadata eventMetadata, RouteContext routeContext)
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

            if (eventMetadata.QueryParameterName == null)
            {
                _logger.LogWarning(
                    0,
                    "A '{ReceiverName}' WebHook request must contain a '{HeaderName}' HTTP header " +
                    "indicating the type of event.",
                    eventMetadata.ReceiverName,
                    eventMetadata.HeaderName);
            }
            else if (eventMetadata.HeaderName == null)
            {
                _logger.LogWarning(
                    1,
                    "A '{ReceiverName}' WebHook request must contain a '{QueryParameterKey}' query " +
                    "parameter indicating the type of event.",
                    eventMetadata.ReceiverName,
                    eventMetadata.QueryParameterName);
            }
            else
            {
                _logger.LogWarning(
                    2,
                    "A '{ReceiverName}' WebHook request must contain a '{HeaderName}' HTTP header or a " +
                    "'{QueryParameterKey}' query parameter indicating the type of event.",
                    eventMetadata.ReceiverName,
                    eventMetadata.HeaderName,
                    eventMetadata.QueryParameterName);
            }

            return false;
        }
    }
}
