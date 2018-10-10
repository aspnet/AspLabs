// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> implementation which uses WebHook ids to select candidate actions.
    /// </summary>
    public class WebHookIdConstraint : IActionConstraint
    {
        private readonly string _id;

        /// <summary>
        /// Instantiates a new <see cref="WebHookIdConstraint"/> with the given <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The receiver id to match.</param>
        public WebHookIdConstraint(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            _id = id;
        }

        /// <summary>
        /// Gets the <see cref="IActionConstraint.Order"/> value used in all <see cref="WebHookIdConstraint"/>
        /// instances.
        /// </summary>
        /// <value>Chosen to run this constraint just after <see cref="WebHookReceiverNameConstraint"/>.</value>
        public static int Order => WebHookReceiverNameConstraint.Order + 10;

        /// <inheritdoc />
        int IActionConstraint.Order => Order;

        /// <inheritdoc />
        public bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.RouteContext.RouteData.TryGetWebHookReceiverId(context.CurrentCandidate.Action, out var id))
            {
                return false;
            }

            return string.Equals(_id, id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
