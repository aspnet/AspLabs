// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names and values used in Trello receivers and handlers.
    /// </summary>
    public static class TrelloConstants
    {
        /// <summary>
        /// Gets the name of the Trello WebHook receiver.
        /// </summary>
        public static string ReceiverName => "trello";

        /// <summary>
        /// Gets the only supported event name for this receiver. This value may be model bound but cannot be used in
        /// action selection.
        /// </summary>
        public static string EventName => "change";

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMinLength => 32;

        /// <summary>
        /// Gets the maximum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMaxLength => 128;

        /// <summary>
        /// Gets the name of the HTTP header that contains the (hex-encoded) signature of the request.
        /// </summary>
        public static string SignatureHeaderName => "X-Trello-WebHook";
    }
}
