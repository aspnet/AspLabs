// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Dropbox receiver.
    /// </summary>
    public class DropboxMetadata :
        WebHookMetadata,
        IWebHookEventMetadata,
        IWebHookFilterMetadata,
        IWebHookGetHeadRequestMetadata
    {
        private readonly DropboxVerifySignatureFilter _verifySignatureFilter;

        /// <summary>
        /// Instantiates a new <see cref="DropboxMetadata"/> instance.
        /// </summary>
        /// <param name="verifySignatureFilter">The <see cref="DropboxVerifySignatureFilter"/>.</param>
        public DropboxMetadata(DropboxVerifySignatureFilter verifySignatureFilter)
            : base(DropboxConstants.ReceiverName)
        {
            _verifySignatureFilter = verifySignatureFilter;
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookEventMetadata...

        /// <inheritdoc />
        public string ConstantValue => DropboxConstants.EventName;

        /// <inheritdoc />
        public string HeaderName => null;

        /// <inheritdoc />
        public string QueryParameterName => null;

        // IWebHookGetHeadRequestMetadata...

        /// <inheritdoc />
        public bool AllowHeadRequests => false;

        /// <inheritdoc />
        public string ChallengeQueryParameterName => DropboxConstants.ChallengeQueryParameterName;

        /// <inheritdoc />
        public int SecretKeyMinLength => DropboxConstants.SecretKeyMinLength;

        /// <inheritdoc />
        public int SecretKeyMaxLength => DropboxConstants.SecretKeyMaxLength;

        // IWebHookFilterMetadata...

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_verifySignatureFilter);
        }
    }
}
