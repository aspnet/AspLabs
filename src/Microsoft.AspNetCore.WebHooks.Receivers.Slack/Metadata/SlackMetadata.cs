// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.WebHooks.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Slack receiver.
    /// </summary>
    public class SlackMetadata : WebHookMetadata, IWebHookBindingMetadata, IWebHookFilterMetadata
    {
        private readonly SlackVerifyTokenFilter _verifyTokenFilter;

        /// <summary>
        /// Instantiates a new <see cref="SlackMetadata"/> instance.
        /// </summary>
        /// <param name="verifyTokenFilter">The <see cref="SlackVerifyTokenFilter"/>.</param>
        public SlackMetadata(SlackVerifyTokenFilter verifyTokenFilter)
            : base(SlackConstants.ReceiverName)
        {
            _verifyTokenFilter = verifyTokenFilter;
        }

        // IWebHookBindingMetadata...

        /// <inheritdoc />
        public IReadOnlyList<WebHookParameter> Parameters { get; } = new List<WebHookParameter>
        {
            new WebHookParameter(
                SlackConstants.SubtextParameterName,
                WebHookParameterType.RouteValue,
                SlackConstants.SubtextRequestKeyName,
                isRequired: false),
        };

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Form;

        // IWebHookFilterMetadata...

        /// <inheritdoc />
        public void AddFilters(WebHookFilterMetadataContext context)
        {
            context.Results.Add(_verifyTokenFilter);
        }
    }
}
