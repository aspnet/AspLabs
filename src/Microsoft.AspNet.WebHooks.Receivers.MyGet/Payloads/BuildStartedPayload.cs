// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Payload sent when a build is started.
    /// See <c>https://docs.myget.org/docs/reference/webhooks</c> for details.
    /// </summary>
    public class BuildStartedPayload
    {
        /// <summary>
        /// Containing feed.
        /// </summary>
        public string FeedIdentifier { get; set; }

        /// <summary>
        /// Containing feed URL.
        /// </summary>
        public Uri FeedUrl { get; set; }

        /// <summary>
        /// Name of the build source.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Branch.
        /// </summary>
        public string Branch { get; set; }
    }
}