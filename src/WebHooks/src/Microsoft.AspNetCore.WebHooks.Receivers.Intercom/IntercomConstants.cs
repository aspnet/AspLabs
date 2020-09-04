// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names and values used in Intercom receivers and handlers.
    /// </summary>
    public static class IntercomConstants
    {
        /// <summary>
        /// Gets the name of the Intercom WebHook receiver.
        /// </summary>
        public static string ReceiverName => "intercom";

        /// <summary>
        /// Gets the name of the Intercom ping event.
        /// </summary>
        public static string PingEventName => "ping";

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMinLength => 16;

        /// <summary>
        /// Gets the key of the hex-encoded signature in the <see cref="SignatureHeaderName"/> value.
        /// </summary>
        public static string SignatureHeaderKey => "sha1";

        /// <summary>
        /// Gets the name of the HTTP header containing key / value pairs, including the (hex-encoded) signature of the
        /// request.
        /// </summary>
        public static string SignatureHeaderName => "X-Hub-Signature";

        /// <summary>
        /// Gets the JSON path of the property in an Intercom WebHook request body containing the Intercom event
        /// topic. Matches the topic name.
        /// </summary>
        public static string EventBodyPropertyPath => "$['topic']";
    }
}
