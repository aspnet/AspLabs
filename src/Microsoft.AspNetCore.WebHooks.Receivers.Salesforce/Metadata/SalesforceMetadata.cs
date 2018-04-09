// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Salesforce receiver.
    /// </summary>
    public class SalesforceMetadata : WebHookMetadata, IWebHookFilterMetadata
    {
        private readonly SalesforceVerifyOrganizationIdFilter _verifyOrganizationIdFilter;

        /// <summary>
        /// Instantiates a new <see cref="SalesforceMetadata"/> instance.
        /// </summary>
        /// <param name="verifyOrganizationIdFilter">The <see cref="SalesforceVerifyOrganizationIdFilter"/>.</param>
        public SalesforceMetadata(SalesforceVerifyOrganizationIdFilter verifyOrganizationIdFilter)
            : base(SalesforceConstants.ReceiverName)
        {
            _verifyOrganizationIdFilter = verifyOrganizationIdFilter;
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Xml;

        // IWebHookFilterMetadata...

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_verifyOrganizationIdFilter);
        }
    }
}
