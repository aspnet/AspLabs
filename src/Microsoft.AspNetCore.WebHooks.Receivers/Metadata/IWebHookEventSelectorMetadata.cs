// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Metadata describing a WebHook action which accepts only events named <see cref="EventName"/>. Implemented in a
    /// <see cref="WebHookAttribute"/> subclass for receivers that support multiple events and do not place event
    /// information in the request body. Receivers using this metadata must also provide an
    /// <see cref="IWebHookEventMetadata"/> service.
    /// </para>
    /// <para>
    /// The <see cref="Routing.WebHookEventNamesConstraint"/> subclasses perform action selection based on this
    /// metadata.
    /// </para>
    /// </summary>
    public interface IWebHookEventSelectorMetadata : IWebHookMetadata
    {
        /// <summary>
        /// Gets the name of the event the associated controller action accepts.
        /// </summary>
        /// <value>
        /// Should not return an empty string. <see langword="null"/> indicates associated action accepts all events.
        /// </value>
        string EventName { get; }
    }
}
