// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Kudu receiver.
    /// </summary>
    public class KuduMetadata : WebHookMetadata, IWebHookBodyTypeMetadataService, IWebHookVerifyCodeMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="KuduMetadata"/> instance.
        /// </summary>
        public KuduMetadata()
            : base(KuduConstants.ReceiverName)
        {
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Json;
    }
}
