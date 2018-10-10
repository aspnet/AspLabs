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
        IWebHookVerifyCodeMetadata
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
        public IReadOnlyList<WebHookParameter> Parameters { get; } = new WebHookParameter[]
        {
            new WebHookParameter(
                BitbucketConstants.WebHookIdParameterName1,
                WebHookParameterType.Header,
                BitbucketConstants.WebHookIdHeaderName,
                isRequired: true),
            new WebHookParameter(
                BitbucketConstants.WebHookIdParameterName2,
                WebHookParameterType.Header,
                BitbucketConstants.WebHookIdHeaderName,
                isRequired: true),
        };

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookEventMetadata...

        /// <inheritdoc />
        public string ConstantValue => null;

        /// <inheritdoc />
        public string HeaderName => BitbucketConstants.EventHeaderName;

        /// <inheritdoc />
        public string QueryParameterName => null;
    }
}
