// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Slack receiver.
    /// </summary>
    public class SlackMetadata : WebHookMetadata, IWebHookBindingMetadata, IWebHookBodyTypeMetadataService
    {
        /// <summary>
        /// Instantiates a new <see cref="SlackMetadata"/> instance.
        /// </summary>
        public SlackMetadata()
            : base(SlackConstants.ReceiverName)
        {
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
        public WebHookBodyType BodyType => WebHookBodyType.Form;
    }
}
