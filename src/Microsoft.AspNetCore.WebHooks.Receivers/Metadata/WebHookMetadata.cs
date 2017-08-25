// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Base class for <see cref="IWebHookMetadata"/> services.
    /// </summary>
    public abstract class WebHookMetadata : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Instantiates a new <see cref="WebHookMetadata"/> with the given <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        protected WebHookMetadata(string receiverName)
        {
            if (string.IsNullOrEmpty(receiverName))
            {
                throw new ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(receiverName));
            }

            ReceiverName = receiverName;
        }

        /// <inheritdoc />
        public string ReceiverName { get; }

        /// <inheritdoc />
        bool IWebHookReceiver.IsApplicable(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            if (ReceiverName == null)
            {
                return true;
            }

            return string.Equals(ReceiverName, receiverName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
