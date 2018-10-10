// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in WordPress receivers and handlers.
    /// </summary>
    public static class WordPressConstants
    {
        /// <summary>
        /// Gets the name of the property in a WordPress WebHook request entity body (formatted as HTML form
        /// URL-encoded data) containing the WordPress event name.
        /// </summary>
        public static string EventBodyPropertyPath => "hook";

        /// <summary>
        /// Gets the name of the WordPress WebHook receiver.
        /// </summary>
        public static string ReceiverName => "wordpress";
    }
}
