// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names and values used in Pusher receivers and handlers.
    /// </summary>
    public static class PusherConstants
    {
        /// <summary>
        /// Gets the name of the JSON property in a Pusher WebHook request body containing its Unix creation timestamp.
        /// </summary>
        public static string EventRequestCreatedAtPropertyName => "time_ms";

        /// <summary>
        /// Gets the name of the JSON property in a Pusher WebHook request body containing a JSON array holding the
        /// individual event details. Each event object contains an <see cref="EventNamePropertyName"/> property and
        /// usually contains a <see cref="ChannelNamePropertyName"/> property.
        /// </summary>
        public static string EventRequestPropertyContainerName => "events";

        /// <summary>
        /// Gets the name of the JSON property in most Pusher event objects containing the event's channel name.
        /// </summary>
        public static string ChannelNamePropertyName => "channel";

        /// <summary>
        /// Gets the name of the JSON property in a Pusher event object containing the event's name.
        /// </summary>
        public static string EventNamePropertyName => "name";

        /// <summary>
        /// Gets the name of the Pusher WebHook receiver.
        /// </summary>
        public static string ReceiverName => "Pusher";

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMinLength => 8;

        /// <summary>
        /// Gets the maximum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMaxLength => 128;

        /// <summary>
        /// Gets the name of the HTTP header containing the key into the configured lookup table. Named entry in the
        /// table contains the secret key the sender used when generating the <see cref="SignatureHeaderName"/> value.
        /// </summary>
        public static string SignatureKeyHeaderName => "X-Pusher-Key";

        /// <summary>
        /// Gets the name of the HTTP header containing the (hex-encoded) signature of the request.
        /// </summary>
        public static string SignatureHeaderName => "X-Pusher-Signature";
    }
}
