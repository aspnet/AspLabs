// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Provides an abstraction for processing incoming WebHooks from a particular WebHook generator, for example
    /// <c>Dropbox</c>, <c>GitHub</c>, etc. All <see cref="Metadata.IWebHookMetadata"/> services and receiver-specific
    /// filters should implement this interface.
    /// </summary>
    public interface IWebHookReceiver
    {
        /// <summary>
        /// <para>
        /// Gets the case-insensitive name of the WebHook generator that this receiver supports, for example
        /// <c>dropbox</c> or <c>net</c>.
        /// </para>
        /// <para>
        /// The name provided here will map to a URI of the form
        /// '<c>https://{host}/api/webhooks/incoming/{ReceiverName}</c>'.
        /// </para>
        /// </summary>
        /// <value>Should not return an empty string.</value>
        string ReceiverName { get; }

        /// <summary>
        /// Gets an indication that this <see cref="IWebHookReceiver"/> should execute in the current request.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="IWebHookReceiver"/> should execute; <see langword="false"/>
        /// otherwise.
        /// </returns>
        bool IsApplicable(string receiverName);
    }
}
