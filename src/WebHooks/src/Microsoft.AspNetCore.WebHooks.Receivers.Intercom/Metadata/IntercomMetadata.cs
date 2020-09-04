// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Intercom receiver.
    /// </summary>
    public class IntercomMetadata :
        WebHookMetadata,
        IWebHookPingRequestMetadata,
        IWebHookEventFromBodyMetadata,
        IWebHookFilterMetadata
    {
        private readonly IntercomVerifySignatureFilter _verifySignatureFilter;

        /// <summary>
        /// Instantiates a new <see cref="IntercomMetadata"/> instance.
        /// </summary>
        /// <param name="verifySignatureFilter">The <see cref="IntercomVerifySignatureFilter"/>.</param>
        public IntercomMetadata(IntercomVerifySignatureFilter verifySignatureFilter)
            : base(IntercomConstants.ReceiverName)
        {
            _verifySignatureFilter = verifySignatureFilter;
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Json;

        /// <inheritdoc />
        public string QueryParameterName => null;

        // IWebHookPingRequestMetadata...

        /// <inheritdoc />
        public string PingEventName => IntercomConstants.PingEventName;


        // IWebHookEventFromBodyMetadata...

        /// <inheritdoc />
        public bool AllowMissing => false;

        /// <inheritdoc />
        public string BodyPropertyPath => IntercomConstants.EventBodyPropertyPath;

        // IWebHookFilterMetadata...

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_verifySignatureFilter);
        }
    }
}
