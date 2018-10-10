// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Metadata describing the event name for ping requests. Implemented in a <see cref="IWebHookMetadata"/> service
    /// for receivers that accept ping requests.
    /// </para>
    /// <para>
    /// The <see cref="Routing.WebHookEventNameConstraint"/> subclasses select a default action (when a ping request
    /// would not otherwise match) based on this metadata. <see cref="Filters.WebHookPingRequestFilter"/>
    /// short-circuits ping requests based on this metadata.
    /// </para>
    /// </summary>
    public interface IWebHookPingRequestMetadata : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Gets the name of the ping event for this receiver.
        /// </summary>
        /// <value>Should not return an empty string or <see langword="null"/>.</value>
        string PingEventName { get; }
    }
}
