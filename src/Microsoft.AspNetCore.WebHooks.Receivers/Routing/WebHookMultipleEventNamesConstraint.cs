// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> implementation which determines the appropriate
    /// <see cref="IWebHookPingRequestMetadata"/> for a WebHook request. It then uses that metadata and a specified
    /// event name to select candidate actions.
    /// </summary>
    public class WebHookMultipleEventNamesConstraint : WebHookEventNamesConstraint
    {
        private readonly string _eventName;
        private readonly IReadOnlyList<IWebHookPingRequestMetadata> _pingMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNamesConstraint"/> instance with the given
        /// <paramref name="eventName"/> and <paramref name="pingMetadata"/>.
        /// </summary>
        /// <param name="eventName">Name of the event this action expects.</param>
        /// <param name="pingMetadata">The collection of <see cref="IWebHookPingRequestMetadata"/> services.</param>
        public WebHookMultipleEventNamesConstraint(
            string eventName,
            IReadOnlyList<IWebHookPingRequestMetadata> pingMetadata)
        {
            if (eventName == null)
            {
                throw new ArgumentNullException(nameof(eventName));
            }
            if (pingMetadata == null)
            {
                throw new ArgumentNullException(nameof(pingMetadata));
            }

            _eventName = eventName;
            _pingMetadata = pingMetadata;
        }

        /// <inheritdoc />
        public override bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.RouteContext.RouteData.TryGetWebHookReceiverName(out var receiverName))
            {
                var pingMetadata = _pingMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                return Accept(context, _eventName, pingMetadata?.PingEventName);
            }


            // ??? Should we throw if this is reached? Should be impossible given WebHookReceiverExistsConstraint.
            return false;
        }
    }
}
