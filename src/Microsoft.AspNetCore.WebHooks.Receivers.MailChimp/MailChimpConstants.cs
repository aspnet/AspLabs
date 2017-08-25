// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in MailChimp receivers and handlers.
    /// </summary>
    public static class MailChimpConstants
    {
        /// <summary>
        /// Gets the name of the JSON property in a MailChimp WebHook request body containing a value somewhat
        /// analogous to an event name.
        /// </summary>
        public static string EventRequestPropertyName => "type";

        /// <summary>
        /// Gets the name of the MailChimp WebHook receiver.
        /// </summary>
        public static string ReceiverName => "mailchimp";
    }
}
