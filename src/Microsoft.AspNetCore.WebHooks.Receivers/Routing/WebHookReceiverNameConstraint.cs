// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> implementation which uses WebHook receiver names to select candidate
    /// actions.
    /// </summary>
    public class WebHookReceiverNameConstraint : IActionConstraint
    {
        private readonly string _receiverName;

        /// <summary>
        /// Instantiates a new <see cref="WebHookReceiverNameConstraint"/> with the given
        /// <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The receiver name to match.</param>
        public WebHookReceiverNameConstraint(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            _receiverName = receiverName;
        }

        /// <summary>
        /// Gets the <see cref="IActionConstraint.Order"/> value used in all
        /// <see cref="WebHookReceiverNameConstraint"/> instances.
        /// </summary>
        /// <value>Chosen to run this constraint just after <see cref="WebHookReceiverNameConstraint"/>.</value>
        public static int Order => WebHookReceiverExistsConstraint.Order + 10;

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

            return string.Equals(_receiverName, receiverName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
