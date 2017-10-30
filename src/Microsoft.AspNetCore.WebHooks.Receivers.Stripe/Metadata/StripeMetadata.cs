// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Stripe receiver.
    /// </summary>
    public class StripeMetadata : WebHookMetadata, IWebHookRequestMetadataService, IWebHookSecurityMetadata
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Instantiates a new <see cref="StripeMetadata"/> instance.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> used to initialize <see cref="VerifyCodeParameter"/>.
        /// </param>
        public StripeMetadata(IConfiguration configuration)
            : base(StripeConstants.ReceiverName)
        {
            _configuration = configuration;
        }

        // IWebHookRequestMetadataService...

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Json;

        /// <inheritdoc />
        public bool UseHttpContextModelBinder => true;

        // IWebHookSecurityMetadata...

        /// <inheritdoc />
        public bool VerifyCodeParameter => _configuration.IsTrue(StripeConstants.DirectWebHookConfigurationKey);

        /// <inheritdoc />
        public bool ShortCircuitGetRequests => false;

        /// <inheritdoc />
        public WebHookGetRequest WebHookGetRequest => null;
    }
}
