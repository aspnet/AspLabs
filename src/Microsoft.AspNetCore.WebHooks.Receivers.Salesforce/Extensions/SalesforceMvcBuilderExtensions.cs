// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks.Internal;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Salesforce WebHooks in an <see cref="IMvcBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SalesforceMvcBuilderExtensions
    {
        /// <summary>
        /// <para>
        /// Add Salesforce WebHook configuration and services to the specified <paramref name="builder"/>. See
        /// <see href="https://go.microsoft.com/fwlink/?linkid=838587"/> for additional details about Salesforce
        /// WebHook requests.
        /// </para>
        /// <para>
        /// The '<c>WebHooks:SalesforceSoap:SecretKey:default</c>' configuration value contains the secret key for
        /// Salesforce WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/sfsoap</c>'.
        /// '<c>WebHooks:SalesforceSoap:SecretKey:{id}</c>' configuration values contain secret keys for Salesforce
        /// WebHook URIs of the form '<c>https://{host}/api/webhooks/incoming/sfsoap/{id}</c>'. Secret keys are
        /// Salesforce Organization IDs and can be found at <see href="https://www.salesforce.com"/> under
        /// <c>Setup | Company Profile | Company Information</c>.
        /// </para>
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcBuilder AddSalesforceWebHooks(this IMvcBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            SalesforceServiceCollectionSetup.AddSalesforceServices(builder.Services);

            return builder
                .AddXmlSerializerFormatters()
                .AddWebHooks();
        }
    }
}
