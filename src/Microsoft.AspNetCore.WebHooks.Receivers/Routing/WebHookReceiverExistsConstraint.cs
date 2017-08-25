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
    /// An <see cref="IActionConstraint"/> implementation which confirms an <see cref="IWebHookMetadata"/> service
    /// exists describing the receiver for the current request.
    /// </summary>
    public class WebHookReceiverExistsConstraint : IActionConstraint
    {
        private readonly IReadOnlyList<IWebHookReceiver> _receiverMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookReceiverNameConstraint"/> with the given <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        public WebHookReceiverExistsConstraint(IEnumerable<IWebHookMetadata> metadata)
        {
            _receiverMetadata = new List<IWebHookReceiver>(metadata.OfType<IWebHookReceiver>());
        }

        /// <summary>
        /// Gets the <see cref="IActionConstraint.Order"/> value used in all
        /// <see cref="WebHookReceiverExistsConstraint"/> instances.
        /// </summary>
        /// <value>Chosen to run this constraint early in action selection.</value>
        public static int Order => -500;

        /// <inheritdoc />
        int IActionConstraint.Order => Order;

        /// <inheritdoc />
        public bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.RouteContext.RouteData.TryGetReceiverName(out var receiverName))
            {
                return false;
            }

            if (!_receiverMetadata.Any(receiver => receiver.IsApplicable(receiverName)))
            {
                return false;
            }

            context.RouteContext.RouteData.Values[WebHookConstants.ReceiverExistsKeyName] = true;

            return true;
        }
    }
}
