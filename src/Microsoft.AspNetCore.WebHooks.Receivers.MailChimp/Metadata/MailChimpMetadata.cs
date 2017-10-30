// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the MailChimp receiver.
    /// </summary>
    public class MailChimpMetadata : WebHookMetadata, IWebHookRequestMetadataService, IWebHookSecurityMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="MailChimpMetadata"/> instance.
        /// </summary>
        public MailChimpMetadata()
            : base(MailChimpConstants.ReceiverName)
        {
        }

        // IWebHookRequestMetadataService...

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Form;

        // IWebHookSecurityMetadata...

        /// <inheritdoc />
        public bool VerifyCodeParameter => true;

        /// <inheritdoc />
        public bool ShortCircuitGetRequests => true;

        /// <inheritdoc />
        public WebHookGetRequest WebHookGetRequest => null;
    }
}
