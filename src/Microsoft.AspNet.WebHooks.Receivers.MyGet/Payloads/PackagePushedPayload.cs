// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Payload sent when a package is pushed to an upstream feed.
    /// See <c>http://docs.myget.org/docs/reference/webhooks</c> for details.
    /// </summary>
    public class PackagePushedPayload
    {
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
        /// Package details URL.
        /// </summary>
        public Uri PackageDetailsUrl { get; set; }

        /// <summary>
        /// Package download URL.
        /// </summary>
        public Uri PackageDownloadUrl { get; set; }

        /// <summary>
        /// Metadata for the package.
        /// </summary>
        public PackageMetadata PackageMetadata { get; set; }

        /// <summary>
        /// Name of the target package source.
        /// </summary>
        public string TargetPackageSourceName { get; set; }

        /// <summary>
        /// URL of the target package source.
        /// </summary>
        public Uri TargetPackageSourceUrl { get; set; }

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