// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Stripe receiver.
    /// </summary>
    public class StripeMetadata : WebHookMetadata,
        IWebHookBindingMetadata,
        IWebHookRequestMetadataService,
        IWebHookSecurityMetadata
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

        // IWebHookBindingMetadata...

        /// <inheritdoc />
        public IReadOnlyList<WebHookParameter> Parameters { get; } = new List<WebHookParameter>
        {
            new WebHookParameter(
                StripeConstants.NotificationIdParameterName,
                WebHookParameterType.RouteValue,
                StripeConstants.NotificationIdKeyName,
                isRequired: false),
        };

        // IWebHookRequestMetadataService...

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookSecurityMetadata...

        /// <inheritdoc />
        public bool VerifyCodeParameter => _configuration.IsTrue(StripeConstants.DirectWebHookConfigurationKey);

        /// <inheritdoc />
        public bool ShortCircuitGetRequests => false;

        /// <inheritdoc />
        public WebHookGetRequest WebHookGetRequest => null;
    }
}
