// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Stripe receiver.
    /// </summary>
    public class StripeMetadata : WebHookMetadata, IWebHookBindingMetadata, IWebHookFilterMetadata
    {
        private readonly StripeTestEventRequestFilter _testEventRequestFilter;
        private readonly StripeVerifyNotificationIdFilter _verifyNotificationIdFilter;
        private readonly StripeVerifySignatureFilter _verifySignatureFilter;

        /// <summary>
        /// Instantiates a new <see cref="StripeMetadata"/> instance.
        /// </summary>
        /// <param name="testEventRequestFilter">The <see cref="StripeTestEventRequestFilter"/>.</param>
        /// <param name="verifyNotificationIdFilter">The <see cref="StripeVerifyNotificationIdFilter"/>.</param>
        /// <param name="verifySignatureFilter">The <see cref="StripeVerifySignatureFilter"/>.</param>
        public StripeMetadata(
            StripeTestEventRequestFilter testEventRequestFilter,
            StripeVerifyNotificationIdFilter verifyNotificationIdFilter,
            StripeVerifySignatureFilter verifySignatureFilter)
            : base(StripeConstants.ReceiverName)
        {
            _testEventRequestFilter = testEventRequestFilter;
            _verifyNotificationIdFilter = verifyNotificationIdFilter;
            _verifySignatureFilter = verifySignatureFilter;
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

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookFilterMetadata...

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_testEventRequestFilter);
            context.Results.Add(_verifyNotificationIdFilter);
            context.Results.Add(_verifySignatureFilter);
        }
    }
}
