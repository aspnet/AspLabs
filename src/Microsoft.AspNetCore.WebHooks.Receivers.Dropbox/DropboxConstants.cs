// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names and values used in Dropbox receivers and handlers.
    /// </summary>
    public static class DropboxConstants
    {
        /// <summary>
        /// Gets the name of a query parameter containing a value to include in the response to an HTTP GET request.
        /// </summary>
        public static string ChallengeQueryParameterName => "challenge";

        /// <summary>
        /// Gets the only supported event name for this receiver. This value may be model bound but cannot be used in
        /// action selection.
        /// </summary>
        public static string EventName => "change";

        /// <summary>
        /// Gets the name of the Dropbox WebHook receiver.
        /// </summary>
        public static string ReceiverName => "dropbox";

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver. Used to confirm the secret key is
        /// property configured before responding to an HTTP GET request.
        /// </summary>
        public static int SecretKeyMinLength => 15;

        /// <summary>
        /// Gets the name of the HTTP header that contains the (hex-encoded) signature of the request.
        /// </summary>
        public static string SignatureHeaderName => "X-Dropbox-Signature";
    }
}
