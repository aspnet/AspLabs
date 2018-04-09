// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Pusher receiver.
    /// </summary>
    public class PusherMetadata : WebHookMetadata, IWebHookEventFromBodyMetadata, IWebHookFilterMetadata
    {
        private readonly PusherVerifySignatureFilter _verifySignatureFilter;

        /// <summary>
        /// Instantiates a new <see cref="PusherMetadata"/> instance.
        /// </summary>
        /// <param name="verifySignatureFilter">The <see cref="PusherVerifySignatureFilter"/>.</param>
        public PusherMetadata(PusherVerifySignatureFilter verifySignatureFilter)
            : base(PusherConstants.ReceiverName)
        {
            _verifySignatureFilter = verifySignatureFilter;
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookEvenFromBodytMetadata...

        /// <inheritdoc />
        public bool AllowMissing => true;

        /// <inheritdoc />
        public string BodyPropertyPath => PusherConstants.EventBodyPropertyPath;

        // IWebHookFilterMetadata...

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_verifySignatureFilter);
        }
    }
}
