// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> implementation which uses WebHook event names to select candidate actions.
    /// </summary>
    public class WebHookEventNameConstraint : IActionConstraint
    {
        private readonly string _eventName;
        private readonly WebHookMetadataProvider _metadataProvider;
        private readonly IWebHookPingRequestMetadata _pingRequestMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNameConstraint"/> instance to verify the request matches the
        /// given <paramref name="eventName"/>.
        /// </summary>
        /// <param name="eventName">Name of the event this action expects.</param>
        public WebHookEventNameConstraint(string eventName)
        {
            _eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNameConstraint"/> instance to verify the request matches the
        /// given <paramref name="eventName"/> or the receiver's
        /// <see cref="IWebHookPingRequestMetadata.PingEventName"/>. The
        /// <see cref="IWebHookPingRequestMetadata.PingEventName"/> is read from
        /// <paramref name="pingRequestMetadata"/>.
        /// </summary>
        /// <param name="eventName">Name of the event this action expects.</param>
        /// <param name="pingRequestMetadata">The receiver's <see cref="IWebHookPingRequestMetadata"/>.</param>
        public WebHookEventNameConstraint(string eventName, IWebHookPingRequestMetadata pingRequestMetadata)
            : this(eventName)
        {
            _pingRequestMetadata = pingRequestMetadata ?? throw new ArgumentNullException(nameof(pingRequestMetadata));
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNameConstraint"/> instance to verify the request matches the
        /// given <paramref name="eventName"/> or the receiver's
        /// <see cref="IWebHookPingRequestMetadata.PingEventName"/>. The
        /// <see cref="IWebHookPingRequestMetadata.PingEventName"/> is read from metadata found in
        /// <paramref name="metadataProvider"/>.
        /// </summary>
        /// <param name="eventName">Name of the event this action expects.</param>
        /// <param name="metadataProvider">
        /// The <see cref="WebHookMetadataProvider"/> service. Searched for applicable metadata per-request.
        /// </param>
        /// <remarks>This overload is intended for use with <see cref="GeneralWebHookAttribute"/>.</remarks>
        public WebHookEventNameConstraint(string eventName, WebHookMetadataProvider metadataProvider)
            : this(eventName)
        {
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
        }

        // Running this constraint last avoids NotFound responses to ping requests because other actions have different
        // constraints on them.
        /// <inheritdoc />
        /// <value>Chosen to ensure constraints of this type run last.</value>
        public int Order => int.MaxValue;

        /// <inheritdoc />
        public bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var pingRequestMetadata = _pingRequestMetadata;
            if (pingRequestMetadata == null && _metadataProvider != null)
            {
                if (!context.RouteContext.RouteData.TryGetWebHookReceiverName(
                    context.CurrentCandidate.Action,
                    out var receiverName))
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.EventConstraints_NoReceiverName,
                        typeof(WebHookReceiverNameConstraint));
                    throw new InvalidOperationException(message);
                }

                pingRequestMetadata = _metadataProvider.GetPingRequestMetadata(receiverName);
            }

            return Accept(context, _eventName, pingRequestMetadata?.PingEventName);
        }

        private bool Accept(ActionConstraintContext context, string eventName, string pingEventName)
        {
            if (context.RouteContext.RouteData.TryGetWebHookEventNames(out var eventNames))
            {
                if (eventNames.Any(name => string.Equals(eventName, name, StringComparison.OrdinalIgnoreCase)))
                {
                    // Simple case. Request is for the expected event.
                    return true;
                }

                if (pingEventName != null &&
                    eventNames.Any(name => string.Equals(pingEventName, name, StringComparison.OrdinalIgnoreCase)))
                {
                    // Decide if this is the candidate that should support the ping event. Ping events must succeed
                    // even if all actions are for other event names.
                    // 1. Is this the first candidate? If not, never handle ping events.
                    if (context.Candidates[0].Action != context.CurrentCandidate.Action)
                    {
                        return false;
                    }

                    // 2. Does another candidate lack this constraint? If yes, they'll handle ping events.
                    for (var i = 1; i < context.Candidates.Count; i++)
                    {
                        var candidate = context.Candidates[i];
                        if (candidate.Constraints == null ||
                            !candidate.Constraints.Any(constraint => constraint is WebHookEventNameConstraint))
                        {
                            return false;
                        }
                    }

                    // 3. Is another candidate configured to accept this request?
                    for (var i = 1; i < context.Candidates.Count; i++)
                    {
                        var candidate = context.Candidates[i];
                        var constraints = context.Candidates.OfType<WebHookEventNameConstraint>();
                        var innerContext = new ActionConstraintContext
                        {
                            Candidates = context.Candidates,
                            CurrentCandidate = candidate,
                            RouteContext = context.RouteContext,
                        };

                        // Constraints shouldn't actually contain more than one instance. But, doesn't hurt to check.
                        if (constraints.All(constraint => constraint.Accept(innerContext)))
                        {
                            return false;
                        }
                    }

                    // 4. No other candidate accepts ping events. It's up to us.
                    return true;
                }
            }

            return false;
       }
    }
}
