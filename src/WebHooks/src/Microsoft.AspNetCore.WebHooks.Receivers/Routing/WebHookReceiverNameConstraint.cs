// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.WebHooks.Metadata;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// An <see cref="IActionConstraint"/> implementation which uses the WebHook receiver name to select candidate
    /// actions. When the action has an associated <see cref="GeneralWebHookAttribute"/>, confirms the request's
    /// receiver exists i.e. a service implementing <see cref="IWebHookBodyTypeMetadataService"/> exists describing
    /// that receiver.
    /// </summary>
    public class WebHookReceiverNameConstraint : IActionConstraint
    {
        private readonly IWebHookBodyTypeMetadataService _bodyTypeMetadata;
        private readonly WebHookMetadataProvider _metadataProvider;
        private readonly int _order;

        /// <summary>
        /// Instantiates a new <see cref="WebHookReceiverNameConstraint"/> instance to verify the request matches the
        /// given <paramref name="bodyTypeMetadata"/>.
        /// </summary>
        /// <param name="bodyTypeMetadata">The receiver's <see cref="IWebHookBodyTypeMetadataService"/>.</param>
        public WebHookReceiverNameConstraint(IWebHookBodyTypeMetadataService bodyTypeMetadata)
        {
            _bodyTypeMetadata = bodyTypeMetadata;
            _order = Order;
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookReceiverNameConstraint"/> instance to verify the receiver's
        /// <see cref="IWebHookEventMetadata"/>. That metadata is found in <paramref name="metadataProvider"/>.
        /// </summary>
        /// <param name="metadataProvider">
        /// The <see cref="WebHookMetadataProvider"/> service. Searched for applicable metadata per-request.
        /// </param>
        /// <remarks>This overload is intended for use with <see cref="GeneralWebHookAttribute"/>.</remarks>
        public WebHookReceiverNameConstraint(WebHookMetadataProvider metadataProvider)
        {
            _metadataProvider = metadataProvider;
            _order = Order + 5;
        }

        /// <summary>
        /// Gets the minimum <see cref="IActionConstraint.Order"/> value used in all
        /// <see cref="WebHookReceiverNameConstraint"/> instances.
        /// </summary>
        /// <value>Chosen to run this constraint early in action selection.</value>
        /// <remarks>
        /// <see cref="IActionConstraint.Order"/> is <see cref="Order"/> when an instance is instantiated with the
        /// <see cref="WebHookReceiverNameConstraint(IWebHookBodyTypeMetadataService)"/> constructor.
        /// <see cref="IActionConstraint.Order"/> is <c>Order + 5</c> when instantiated with the
        /// <see cref="WebHookReceiverNameConstraint(WebHookMetadataProvider)"/> constructor.
        /// </remarks>
        public static int Order => -500;

        /// <inheritdoc />
        int IActionConstraint.Order => _order;

        /// <inheritdoc />
        public bool Accept(ActionConstraintContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.RouteContext.RouteData.TryGetWebHookReceiverName(
                context.CurrentCandidate.Action,
                out var receiverName))
            {
                return false;
            }

            if (_bodyTypeMetadata == null)
            {
                if (_metadataProvider.GetBodyTypeMetadata(receiverName) == null)
                {
                    // Received a request for (say) https://{host}/api/webhooks/incoming/mine but the "mine" receiver
                    // is not configured. But, probably not a misconfiguration in this application.
                    // WebHookMetadataProvier detects "incomplete" receivers i.e. those with some metadata
                    // services but lacking an IWebHookBodyTypeMetadataService implementation.
                    return false;
                }
            }
            else
            {
                if (!_bodyTypeMetadata.IsApplicable(receiverName))
                {
                    // Received a request for (say) https://{host}/api/webhooks/incoming/their but this action is
                    // configured for the "mine" receiver.
                    return false;
                }
            }

            context.RouteContext.RouteData.Values[WebHookConstants.ReceiverExistsKeyName] = true;
            return true;
        }
    }
}
