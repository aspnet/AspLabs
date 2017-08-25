// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> implementation which determines the appropriate
    /// <see cref="IWebHookEventMetadata"/> for a WebHook request. It then uses that
    /// <see cref="IWebHookEventMetadata"/> to determine the event name. This constraint almost-always accepts all
    /// candidates.
    /// </summary>
    public class WebHookMultipleEventMapperConstraint : WebHookEventMapperConstraint
    {
        private readonly IReadOnlyList<IWebHookEventMetadata> _eventMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookSingleEventMapperConstraint"/> instance with the given
        /// <paramref name="loggerFactory"/> and <paramref name="metadata"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        public WebHookMultipleEventMapperConstraint(
            ILoggerFactory loggerFactory,
            IEnumerable<IWebHookMetadata> metadata)
            : base(loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            _eventMetadata = new List<IWebHookEventMetadata>(metadata.OfType<IWebHookEventMetadata>());
        }

        /// <inheritdoc />
        public override bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeContext = context.RouteContext;
            if (routeContext.RouteData.TryGetReceiverName(out var receiverName))
            {
                var eventMetadata = _eventMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (eventMetadata != null)
                {
                    return Accept(eventMetadata, routeContext);
                }

                // This receiver does not have IWebHookEventMetadata and that's fine.
                return true;
            }

            // ??? Should we throw if this is reached? Should be impossible given WebHookReceiverExistsConstraint.
            return false;
        }
    }
}
