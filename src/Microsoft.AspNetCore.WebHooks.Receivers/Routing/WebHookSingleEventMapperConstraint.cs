// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> implementation which uses one <see cref="IWebHookEventMetadata"/> to
    /// determine the event name for a WebHook request. This constraint almost-always accepts all candidates.
    /// </summary>
    public class WebHookSingleEventMapperConstraint : WebHookEventMapperConstraint
    {
        private readonly IWebHookEventMetadata _eventMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookSingleEventMapperConstraint"/> instance with the given
        /// <paramref name="loggerFactory"/> and <paramref name="eventMetadata"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="eventMetadata">The <see cref="IWebHookEventMetadata"/>.</param>
        public WebHookSingleEventMapperConstraint(ILoggerFactory loggerFactory, IWebHookEventMetadata eventMetadata)
            : base(loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (eventMetadata == null)
            {
                throw new ArgumentNullException(nameof(eventMetadata));
            }

            _eventMetadata = eventMetadata;
        }

        /// <inheritdoc />
        public override bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Accept(_eventMetadata, context.RouteContext);
        }
    }
}
