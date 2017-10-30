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
    /// <see cref="IWebHookEventMetadata"/> for a WebHook request. It then uses that metadata and a specified event
    /// name to select candidate actions.
    /// </summary>
    public class WebHookMultipleEventNamesConstraint : WebHookEventNamesConstraint
    {
        private readonly string _eventName;
        private readonly IEnumerable<IWebHookEventMetadata> _eventMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNamesConstraint"/> instance with the given
        /// <paramref name="eventName"/> and <paramref name="eventMetadata"/>.
        /// </summary>
        /// <param name="eventName">Name of the event this action expects.</param>
        /// <param name="eventMetadata">The collection of <see cref="IWebHookEventMetadata"/> services.</param>
        public WebHookMultipleEventNamesConstraint(
            string eventName,
            IEnumerable<IWebHookEventMetadata> eventMetadata)
        {
            if (eventName == null)
            {
                throw new ArgumentNullException(nameof(eventName));
            }

            _eventName = eventName;
            _eventMetadata = eventMetadata;
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
                var eventMetadata = _eventMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                return Accept(context, _eventName, eventMetadata?.PingEventName);
            }


            // ??? Should we throw if this is reached? Should be impossible given WebHookReceiverExistsConstraint.
            return false;
        }
    }
}
