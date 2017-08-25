// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Provides an abstraction for managing <see cref="IWebHookReceiver"/> configuration. This makes it possible
    /// to manage configuration of secrets in a consistent manner separately of any given
    /// <see cref="IWebHookReceiver"/>.
    /// </summary>
    public interface IWebHookReceiverConfig
    {
        /// <summary>
        /// Gets the application's <see cref="IConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// Primarily for convenience; avoids consumers having to get the <see cref="IConfiguration"/> separately.
        /// </remarks>
        IConfiguration Configuration { get; }

        // ??? Why does this method return a Task? Not needed in our implementation. But, ... extensibility?
        /// <summary>
        /// Gets the receiver configuration for a given <paramref name="configurationName"/> and a particular
        /// <paramref name="id"/> or <see langword="null"/> if not found.
        /// </summary>
        /// <param name="configurationName">
        /// The case-insensitive name of the receiver configuration used by the incoming WebHook. Typically this is the
        /// name of the receiver, e.g. <c>github</c>.
        /// </param>
        /// <param name="id">
        /// A (possibly <see langword="null"/> or empty) ID of a particular configuration for the given
        /// <paramref name="configurationName"/>. This can be used for one receiver to differentiate between multiple
        /// configurations.
        /// </param>
        /// <returns>The requested configuration or <see langword="null"/> if not found.</returns>
        Task<string> GetReceiverConfigAsync(string configurationName, string id);
    }
}
