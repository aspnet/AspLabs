// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in Stripe receivers and handlers.
    /// </summary>
    public static class StripeConstants
    {
        /// <summary>
        /// Gets the name of the JSON property in a Stripe WebHook request body containing its Unix creation
        /// timestamp.
        /// </summary>
        public static string CreatedPropertyName => "created";

        /// <summary>
        /// Gets the name of the JSON property in a Stripe WebHook request body containing the
        /// <see cref="DataDetailsPropertyName"/>.
        /// </summary>
        public static string DataPropertyName => "data";

        /// <summary>
        /// Gets the name of the JSON property in a Stripe WebHook request body containing the event details.
        /// </summary>
        public static string DataDetailsPropertyName => "object";

        /// <summary>
        /// Gets the name of the JSON property in a Stripe WebHook request body containing the event name.
        /// </summary>
        public static string EventPropertyName => "type";

        /// <summary>
        /// Gets the name of the <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry containing the Stripe
        /// notification identifier for the current request.
        /// </summary>
        /// <seealso cref="NotificationIdPropertyName"/>
        public static string NotificationIdKeyName => "notificationId";

        /// <summary>
        /// Gets the name of a parameter bound to the Stripe notification identifier for the current request.
        /// </summary>
        /// <seealso cref="NotificationIdKeyName"/>
        public static string NotificationIdParameterName => "notificationId";

        /// <summary>
        /// Gets the name of the JSON property in a Stripe WebHook request body containing the notification
        /// identifier.
        /// </summary>
        public static string NotificationIdPropertyName => "id";

        /// <summary>
        /// Gets the name of the JSON property in a Stripe WebHook request body indicating it carries a live mode
        /// event.
        /// </summary>
        /// <value><see langword="true"/> if this is a live mode event; otherwise, this is a test mode event.</value>
        /// <remarks>
        /// Stripe sends test mode events i.e. uses test data with notification identifiers other than
        /// <see cref="TestNotificationId"/>.
        /// </remarks>
        public static string LiveModePropertyName => "livemode";

        /// <summary>
        /// Gets the key of a configuration value indicating the Stripe receiver should flow test events to Stripe
        /// actions. Configuration value, if any, should parse as a <see cref="bool"/>. If that parsed configuration
        /// value is <see langword="true"/>, the action will receive test events. Otherwise, the receiver
        /// short-circuits test events.
        /// </summary>
        public static string PassThroughTestEventsConfigurationKey { get; } = ConfigurationPath.Combine(
            WebHookConstants.ReceiverConfigurationSectionKey,
            ReceiverName,
            "PassThroughTestEvents");

        /// <summary>
        /// Gets the name of the Stripe WebHook receiver.
        /// </summary>
        public static string ReceiverName => "stripe";

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMinLength => 16;

        /// <summary>
        /// Gets the maximum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMaxLength => 128;

        /// <summary>
        /// Gets the name of the HTTP header that contains the timestamp and signature of the request. See
        /// <see href="https://stripe.com/docs/webhooks#replay-attacks"/> and
        /// <see href="https://stripe.com/docs/webhooks#verify-manually"/> for more information about this header.
        /// </summary>
        public static string SignatureHeaderName => "Stripe-Signature";

        /// <summary>
        /// Gets the key of the signature value(s) in the <see cref="SignatureHeaderName"/>.
        /// </summary>
        public static string SignatureKey => "v1";

        /// <summary>
        /// Gets the notification identifier used in test Stripe WebHook requests.
        /// </summary>
        /// <remarks>
        /// Stripe does not use this notification identifier when for example sending an event about a test payment.
        /// </remarks>
        /// <seealso cref="NotificationIdPropertyName"/>
        public static string TestNotificationId => "evt_00000000000000";

        /// <summary>
        /// Gets the key of the timestamp value in the <see cref="SignatureHeaderName"/>.
        /// </summary>
        public static string TimestampKey => "t";
    }
}
