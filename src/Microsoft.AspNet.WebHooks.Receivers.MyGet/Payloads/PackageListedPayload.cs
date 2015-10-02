// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Payload sent when a package is listed/unlisted.
    /// See <c>http://docs.myget.org/docs/reference/webhooks</c> for details.
    /// </summary>
    public class PackageListedPayload
    {
        /// <summary>
        /// Action performed. Will contain "listed" or "unlisted".
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Package type.
        /// </summary>
        public string PackageType { get; set; }

        /// <summary>
        ///  Package identifier.
        /// </summary>
        public string PackageIdentifier { get; set; }

        /// <summary>
        /// Package version.
        /// </summary>
        public string PackageVersion { get; set; }

        /// <summary>
        /// Containing feed.
        /// </summary>
        public string FeedIdentifier { get; set; }

        /// <summary>
        /// Containing feed URL.
        /// </summary>
        public Uri FeedUrl { get; set; }
    }
}