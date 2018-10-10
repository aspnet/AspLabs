// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the GitHub receiver.
    /// </summary>
    public class GitHubMetadata :
        WebHookMetadata,
        IWebHookEventMetadata,
        IWebHookFilterMetadata,
        IWebHookPingRequestMetadata
    {
        private readonly GitHubVerifySignatureFilter _verifySignatureFilter;

        /// <summary>
        /// Instantiates a new <see cref="GitHubMetadata"/> instance.
        /// </summary>
        /// <param name="verifySignatureFilter">The <see cref="GitHubVerifySignatureFilter"/>.</param>
        public GitHubMetadata(GitHubVerifySignatureFilter verifySignatureFilter)
            : base(GitHubConstants.ReceiverName)
        {
            _verifySignatureFilter = verifySignatureFilter;
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookEventMetadata...

        /// <inheritdoc />
        public string ConstantValue => null;

        /// <inheritdoc />
        public string HeaderName => GitHubConstants.EventHeaderName;

        /// <inheritdoc />
        public string QueryParameterName => null;

        // IWebHookPingRequestMetadata...

        /// <inheritdoc />
        public string PingEventName => GitHubConstants.PingEventName;

        // IWebHookFilterMetadata...

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_verifySignatureFilter);
        }
    }
}
