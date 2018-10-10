// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in Kudu receivers and handlers.
    /// </summary>
    public static class KuduConstants
    {
        /// <summary>
        /// Gets the JSON path of the property in a Kudu WebHook request body containing the Kudu event name.
        /// </summary>
        public static string EventBodyPropertyPath => "$.status";

        /// <summary>
        /// Gets the name of the Kudu WebHook receiver.
        /// </summary>
        public static string ReceiverName => "kudu";
    }
}
