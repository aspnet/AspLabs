// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Bitbucket receiver.
    /// </summary>
    public class BitbucketMetadata :
        WebHookMetadata,
        IWebHookBindingMetadata,
        IWebHookEventMetadata,
        IWebHookRequestMetadataService,
        IWebHookSecurityMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="BitbucketMetadata"/> instance.
        /// </summary>
        public BitbucketMetadata()
            : base(BitbucketConstants.ReceiverName)
        {
        }

        // IWebHookBindingMetadata...

        /// <inheritdoc />
        public IReadOnlyList<WebHookParameter> Parameters { get; } = new List<WebHookParameter>
        {
            new WebHookParameter(
                BitbucketConstants.WebHookIdParameterName1,
                BitbucketConstants.WebHookIdHeaderName,
                isQueryParameter: false,
                isRequired: true),
            new WebHookParameter(
                BitbucketConstants.WebHookIdParameterName2,
                BitbucketConstants.WebHookIdHeaderName,
                isQueryParameter: false,
                isRequired: true),
        };

        // IWebHookEventMetadata...

        /// <inheritdoc />
        public string ConstantValue => null;

        /// <inheritdoc />
        public string HeaderName => BitbucketConstants.EventHeaderName;

        /// <inheritdoc />
        public string PingEventName => null;

        /// <inheritdoc />
        public string QueryParameterName => null;

        // IWebHookRequestMetadataService...

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Json;

        /// <inheritdoc />
        public bool UseHttpContextModelBinder => false;

        // IWebHookSecurityMetadata...

        /// <inheritdoc />
        public bool VerifyCodeParameter => true;

        /// <inheritdoc />
        public bool ShortCircuitGetRequests => false;

        /// <inheritdoc />
        public WebHookGetRequest WebHookGetRequest => null;
    }
}
