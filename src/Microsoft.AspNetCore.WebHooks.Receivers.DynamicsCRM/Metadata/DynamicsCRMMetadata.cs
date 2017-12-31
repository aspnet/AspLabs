// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Dynamics CRM receiver.
    /// </summary>
    public class DynamicsCRMMetadata :
        WebHookMetadata,
        IWebHookBodyTypeMetadataService,
        IWebHookEventFromBodyMetadata,
        IWebHookVerifyCodeMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="DynamicsCRMMetadata"/> instance.
        /// </summary>
        public DynamicsCRMMetadata()
            : base(DynamicsCRMConstants.ReceiverName)
        {
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookEventFromBodyMetadata...

        /// <inheritdoc />
        public bool AllowMissing => true;

        /// <inheritdoc />
        public string BodyPropertyPath => DynamicsCRMConstants.EventBodyPropertyPath;
    }
}
