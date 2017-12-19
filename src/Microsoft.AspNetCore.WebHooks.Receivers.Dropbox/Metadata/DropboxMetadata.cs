// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Dropbox receiver.
    /// </summary>
    public class DropboxMetadata :
        WebHookMetadata,
        IWebHookBodyTypeMetadataService,
        IWebHookEventMetadata,
        IWebHookGetRequestMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="DropboxMetadata"/> instance.
        /// </summary>
        public DropboxMetadata()
            : base(DropboxConstants.ReceiverName)
        {
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookEventMetadata...

        /// <inheritdoc />
        public string ConstantValue => DropboxConstants.EventName;

        /// <inheritdoc />
        public string HeaderName => null;

        /// <inheritdoc />
        public string QueryParameterName => null;

        // IWebHookGetRequestMetadata...

        /// <inheritdoc />
        public string ChallengeQueryParameterName => DropboxConstants.ChallengeQueryParameterName;

        /// <inheritdoc />
        public int SecretKeyMinLength => DropboxConstants.SecretKeyMinLength;

        /// <inheritdoc />
        public int SecretKeyMaxLength => DropboxConstants.SecretKeyMaxLength;
    }
}
