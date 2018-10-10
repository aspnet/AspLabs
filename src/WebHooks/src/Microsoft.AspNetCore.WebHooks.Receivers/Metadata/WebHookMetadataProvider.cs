// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Abstraction for a service providing <see cref="IWebHookMetadata"/> from other registered services. Used for
    /// runtime lookups when the selected action has an associated <see cref="GeneralWebHookAttribute"/>.
    /// </summary>
    public abstract class WebHookMetadataProvider
    {
        /// <summary>
        /// Gets the <see cref="IWebHookBindingMetadata"/> for the receiver with given <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// If the metadata exists, the <see cref="IWebHookBindingMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>; <see langword="null"/> otherwise.
        /// </returns>
        public abstract IWebHookBindingMetadata GetBindingMetadata(string receiverName);

        /// <summary>
        /// Gets the <see cref="IWebHookBodyTypeMetadataService"/> for the receiver with given
        /// <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// If the metadata exists, the <see cref="IWebHookBodyTypeMetadataService"/> for the receiver with given
        /// <paramref name="receiverName"/>; <see langword="null"/> otherwise.
        /// </returns>
        /// <remarks>
        /// Does not throw on missing metadata though an <see cref="IWebHookReceiver"/> lacking a registered
        /// <see cref="IWebHookBodyTypeMetadataService"/> implementation is invalid. That error is handled elsewhere.
        /// </remarks>
        public abstract IWebHookBodyTypeMetadataService GetBodyTypeMetadata(string receiverName);

        /// <summary>
        /// Gets the <see cref="IWebHookEventFromBodyMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// If the metadata exists, the <see cref="IWebHookEventFromBodyMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>; <see langword="null"/> otherwise.
        /// </returns>
        public abstract IWebHookEventFromBodyMetadata GetEventFromBodyMetadata(string receiverName);

        /// <summary>
        /// Gets the <see cref="IWebHookEventMetadata"/> for the receiver with given <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// If the metadata exists, the <see cref="IWebHookEventMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>; <see langword="null"/> otherwise.
        /// </returns>
        public abstract IWebHookEventMetadata GetEventMetadata(string receiverName);

        /// <summary>
        /// Gets the <see cref="IWebHookFilterMetadata"/> for the receiver with given <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// If the metadata exists, the <see cref="IWebHookFilterMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>; <see langword="null"/> otherwise.
        /// </returns>
        public abstract IWebHookFilterMetadata GetFilterMetadata(string receiverName);

        /// <summary>
        /// Gets the <see cref="IWebHookGetHeadRequestMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// If the metadata exists, the <see cref="IWebHookGetHeadRequestMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>; <see langword="null"/> otherwise.
        /// </returns>
        public abstract IWebHookGetHeadRequestMetadata GetGetHeadRequestMetadata(string receiverName);

        /// <summary>
        /// Gets the <see cref="IWebHookPingRequestMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// If the metadata exists, the <see cref="IWebHookPingRequestMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>; <see langword="null"/> otherwise.
        /// </returns>
        public abstract IWebHookPingRequestMetadata GetPingRequestMetadata(string receiverName);

        /// <summary>
        /// Gets the <see cref="IWebHookVerifyCodeMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// If the metadata exists, the <see cref="IWebHookVerifyCodeMetadata"/> for the receiver with given
        /// <paramref name="receiverName"/>; <see langword="null"/> otherwise.
        /// </returns>
        public abstract IWebHookVerifyCodeMetadata GetVerifyCodeMetadata(string receiverName);
    }
}
