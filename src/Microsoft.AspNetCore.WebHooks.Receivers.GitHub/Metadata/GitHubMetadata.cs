// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the GitHub receiver.
    /// </summary>
    public class GitHubMetadata : WebHookMetadata, IWebHookEventMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="GitHubMetadata"/> instance.
        /// </summary>
        public GitHubMetadata()
            : base(GitHubConstants.ReceiverName)
        {
        }

        /// <inheritdoc />
        public string ConstantValue => null;

        /// <inheritdoc />
        public string HeaderName => GitHubConstants.EventHeaderName;

        /// <inheritdoc />
        public string PingEventName => GitHubConstants.PingEventName;

        /// <inheritdoc />
        public string QueryParameterName => null;
    }
}
