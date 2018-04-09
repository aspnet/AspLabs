// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Trello receiver.
    /// </summary>
    public class TrelloMetadata :
        WebHookMetadata,
        IWebHookEventMetadata,
        IWebHookFilterMetadata,
        IWebHookGetHeadRequestMetadata
    {
        private readonly TrelloVerifySignatureFilter _verifySignatureFilter;

        /// <summary>
        /// Instantiates a new <see cref="TrelloMetadata"/> instance.
        /// </summary>
        /// <param name="verifySignatureFilter">The <see cref="TrelloVerifySignatureFilter"/>.</param>
        public TrelloMetadata(TrelloVerifySignatureFilter verifySignatureFilter)
            : base(TrelloConstants.ReceiverName)
        {
            _verifySignatureFilter = verifySignatureFilter;
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Json;

        // IWebHookEventMetadata...

        /// <inheritdoc />
        public string ConstantValue => TrelloConstants.EventName;

        /// <inheritdoc />
        public string HeaderName => null;

        /// <inheritdoc />
        public string QueryParameterName => null;

        // IWebHookGetHeadRequestMetadata...

        /// <inheritdoc />
        public bool AllowHeadRequests => true;

        /// <inheritdoc />
        public string ChallengeQueryParameterName => null;

        /// <inheritdoc />
        public int SecretKeyMinLength => TrelloConstants.SecretKeyMinLength;

        /// <inheritdoc />
        public int SecretKeyMaxLength => TrelloConstants.SecretKeyMaxLength;

        // IWebHookFilterMetadata...

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_verifySignatureFilter);
        }
    }
}
