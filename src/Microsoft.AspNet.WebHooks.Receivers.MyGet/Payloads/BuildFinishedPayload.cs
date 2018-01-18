// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Payload sent when a build has finished.
    /// See <c>https://docs.myget.org/docs/reference/webhooks</c> for details.
    /// </summary>
    public class BuildFinishedPayload
    {
        private readonly Collection<Package> _packages = new Collection<Package>();

        /// <summary>
        /// Gets or sets the name of the build source.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Branch.
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// Build result. Will contain "failed" or "success".
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Feed.
        /// </summary>
        public string FeedIdentifier { get; set; }

        /// <summary>
        /// Feed URL.
        /// </summary>
        public Uri FeedUrl { get; set; }

        /// <summary>
        /// URL to the build log.
        /// </summary>
        public Uri BuildLogUrl { get; set; }

        /// <summary>
        /// Gets the packages that have been created.
        /// </summary>
        public Collection<Package> Packages
        {
            get { return _packages; }
        }
    }
}