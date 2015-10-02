// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Describes a Package being manipulated.
    /// See <c>http://docs.myget.org/docs/reference/webhooks</c> for details.
    /// </summary>
    public class Package
    {
        /// <summary>
        /// Type of the package.
        /// </summary>
        public string PackageType { get; set; }

        /// <summary>
        /// Package identifier.
        /// </summary>
        public string PackageIdentifier { get; set; }

        /// <summary>
        /// Package version.
        /// </summary>
        public string PackageVersion { get; set; }

        /// <summary>
        /// Target framework, if applicable.
        /// </summary>
        public string TargetFramework { get; set; }
    }
}
