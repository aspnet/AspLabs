// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Stripe receiver.
    /// </summary>
    public class StripeMetadata : WebHookMetadata, IWebHookRequestMetadataService, IWebHookSecurityMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="StripeMetadata"/> instance.
        /// </summary>
        public StripeMetadata(IWebHookReceiverConfig receiverConfig)
            : base(StripeConstants.ReceiverName)
        {
            VerifyCodeParameter = receiverConfig.IsTrue(StripeConstants.DirectWebHookConfigurationKey);
        }

        // IWebHookRequestMetadataService...

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Json;

        /// <inheritdoc />
        public bool UseHttpContextModelBinder => true;

        // IWebHookSecurityMetadata...

        /// <inheritdoc />
        public bool VerifyCodeParameter { get; }

        /// <inheritdoc />
        public bool ShortCircuitGetRequests => false;

        /// <inheritdoc />
        public WebHookGetRequest WebHookGetRequest => null;
    }
}
