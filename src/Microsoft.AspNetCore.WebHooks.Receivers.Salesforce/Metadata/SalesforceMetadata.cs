// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Salesforce receiver.
    /// </summary>
    public class SalesforceMetadata : WebHookMetadata, IWebHookRequestMetadataService
    {
        /// <summary>
        /// Instantiates a new <see cref="SalesforceMetadata"/> instance.
        /// </summary>
        public SalesforceMetadata()
            : base(SalesforceConstants.ReceiverName)
        {
        }

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Xml;
    }
}
