// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in Bitbucket receivers and handlers.
    /// </summary>
    public static class BitbucketConstants
    {
        /// <summary>
        /// Gets the name of the header containing the Bitbucket event name e.g. <c>repo:push</c> or
        /// <c>issue:created</c>.
        /// </summary>
        public static string EventHeaderName => "X-Event-Key";

        /// <summary>
        /// Gets the name of the Bitbucket WebHook receiver.
        /// </summary>
        public static string ReceiverName => "bitbucket";

        /// <summary>
        /// Gets the name of the header containing the Bitbucket WebHook UUID.
        /// </summary>
        public static string WebHookIdHeaderName => "X-Hook-UUID";

        /// <summary>
        /// Gets the name of one parameter bound to the <see cref="WebHookIdHeaderName"/> header.
        /// </summary>
        public static string WebHookIdParameterName1 => "webHookid";

        /// <summary>
        /// Gets the name of another parameter bound to the <see cref="WebHookIdHeaderName"/> header.
        /// </summary>
        public static string WebHookIdParameterName2 => "webHook_id";
    }
}
