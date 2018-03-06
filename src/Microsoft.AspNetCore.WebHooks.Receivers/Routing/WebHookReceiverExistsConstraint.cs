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
        private readonly IReadOnlyList<IWebHookBodyTypeMetadataService> _bodyTypeMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookReceiverNameConstraint"/> instance.
        /// </summary>
        /// <param name="bodyTypeMetadata">
        /// The collection of <see cref="IWebHookBodyTypeMetadataService"/> services.
        /// </param>
        public WebHookReceiverExistsConstraint(IEnumerable<IWebHookBodyTypeMetadataService> bodyTypeMetadata)
        {
            _bodyTypeMetadata = bodyTypeMetadata.ToArray();
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

            if (!context.RouteContext.RouteData.TryGetWebHookReceiverName(out var receiverName))
            {
                return false;
            }

            if (!_bodyTypeMetadata.Any(metadata => metadata.IsApplicable(receiverName)))
            {
                // Received a request for (say) https://{host}/api/webhooks/incoming/mine but the "mine" receiver
                // is not configured. Not necessarily a misconfiguration in this application.
                //
                // WebHookMetadataProvider throws if it encounters a receiver that does not register an
                // IWebHookBodyTypeMetadataService implementation. The provider only does this if the application uses
                // a receiver's specific attribute. This constraint handles the remaining case, ensuring requests for
                // such a mis configured receiver do not reach [GeneralWebHook] actions. (May be nice to have extra
                // logging for a mis-configured receiver over a non-existent one. But, that would require checks for
                // every other metadata type.)
                return false;
            }

            context.RouteContext.RouteData.Values[WebHookConstants.ReceiverExistsKeyName] = true;

            return true;
        }
    }
}
