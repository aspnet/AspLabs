// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the MailChimp receiver.
    /// </summary>
    public class MailChimpMetadata :
        WebHookMetadata,
        IWebHookEventFromBodyMetadata,
        IWebHookGetHeadRequestMetadata,
        IWebHookVerifyCodeMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="MailChimpMetadata"/> instance.
        /// </summary>
        public MailChimpMetadata()
            : base(MailChimpConstants.ReceiverName)
        {
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Form;

        // IWebHookEventFromBodyMetadata...

        /// <inheritdoc />
        public bool AllowMissing => false;

        /// <inheritdoc />
        public string BodyPropertyPath => MailChimpConstants.EventBodyPropertyName;

        // IWebHookGetHeadRequestMetadata...

        /// <inheritdoc />
        public bool AllowHeadRequests => false;

        /// <inheritdoc />
        /// <inheritdoc />
        public string ChallengeQueryParameterName => null;

        /// <inheritdoc />
        public int SecretKeyMinLength => WebHookConstants.CodeParameterMinLength;

        /// <inheritdoc />
        public int SecretKeyMaxLength => WebHookConstants.CodeParameterMaxLength;
    }
}
