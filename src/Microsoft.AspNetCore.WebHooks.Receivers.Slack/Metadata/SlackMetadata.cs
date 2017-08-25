// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the Slack receiver.
    /// </summary>
    public class SlackMetadata : WebHookMetadata, IWebHookRequestMetadataService
    {
        /// <summary>
        /// Instantiates a new <see cref="SlackMetadata"/> instance.
        /// </summary>
        public SlackMetadata()
            : base(SlackConstants.ReceiverName)
        {
        }

        /// <inheritdoc />
        public WebHookBodyType BodyType => WebHookBodyType.Form;

        /// <inheritdoc />
        public bool UseHttpContextModelBinder => true;
    }
}
