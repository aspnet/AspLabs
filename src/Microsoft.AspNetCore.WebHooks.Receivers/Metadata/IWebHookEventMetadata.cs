// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Metadata describing the source of a WebHook <see cref="string"/> or <c>string[]</c> action parameter named
    /// <c>action</c>, <c>@event</c>, <c>eventNames</c>, or similar. Implemented in a <see cref="IWebHookMetadata"/>
    /// service for receivers that do not place event names in the request body or special-case event name mapping.
    /// </para>
    /// <para>
    /// <see cref="Routing.WebHookEventMapperConstraint"/> maps event names based on this metadata.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Receivers must not provide both this metadata and an <see cref="IWebHookEventFromBodyMetadata"/> service.
    /// </remarks>
    public interface IWebHookEventMetadata : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Gets the constant event name for this receiver. Used as a fallback when <see cref="HeaderName"/> and
        /// <see cref="QueryParameterName"/> are <see langword="null"/> or do not match the request.
        /// </summary>
        /// <value>Should not return an empty string.</value>
        string ConstantValue { get; }

        /// <summary>
        /// Gets the name of the header containing event name(s) for this receiver. The named header is checked before
        /// <see cref="QueryParameterName"/> if both are non-<see langword="null"/>.
        /// </summary>
        /// <value>Should not return an empty string.</value>
        string HeaderName { get; }

        /// <summary>
        /// Gets the name of the query parameter containing event name(s) for this receiver. The named query parameter
        /// is checked after <see cref="HeaderName"/> if both are non-<see langword="null"/>.
        /// </summary>
        /// <value>Should not return an empty string.</value>
        string QueryParameterName { get; }
    }
}
