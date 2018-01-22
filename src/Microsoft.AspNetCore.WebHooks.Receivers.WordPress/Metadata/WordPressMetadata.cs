// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// An <see cref="IWebHookMetadata"/> service containing metadata about the WordPress receiver.
    /// </summary>
    public class WordPressMetadata : WebHookMetadata, IWebHookEventFromBodyMetadata, IWebHookVerifyCodeMetadata
    {
        /// <summary>
        /// Instantiates a new <see cref="WordPressMetadata"/> instance.
        /// </summary>
        public WordPressMetadata()
            : base(WordPressConstants.ReceiverName)
        {
        }

        // IWebHookBodyTypeMetadataService...

        /// <inheritdoc />
        public override WebHookBodyType BodyType => WebHookBodyType.Form;

        // IWebHookEventFromBodyMetadata...

        /// <inheritdoc />
        public bool AllowMissing => false;

        /// <inheritdoc />
        public string BodyPropertyPath => WordPressConstants.EventBodyPropertyPath;
    }
}
