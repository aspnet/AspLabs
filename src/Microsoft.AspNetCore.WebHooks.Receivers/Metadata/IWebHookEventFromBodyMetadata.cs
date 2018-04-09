// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Metadata describing the source within the request body of a WebHook <see cref="string"/> or <c>string[]</c>
    /// action parameter named action parameter named <c>action</c>, <c>@event</c>, <c>eventNames</c>, or similar.
    /// Implemented in a <see cref="IWebHookMetadata"/> service for receivers that place event names in the body and
    /// do not special-case event name mapping.
    /// </para>
    /// <para>
    /// <see cref="Filters.WebHookEventNameMapperFilter"/> maps event names based on this metadata.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Receivers must not provide both this metadata and an <see cref="IWebHookEventMetadata"/> service.
    /// </remarks>
    public interface IWebHookEventFromBodyMetadata : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Gets an indication whether missing event names (no <see cref="BodyPropertyPath"/> match in the request)
        /// should be allowed.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, requests lacking event names are allowed. Otherwise,
        /// <see cref="Filters.WebHookEventNameMapperFilter"/> short-circuits the request, responding with a 400 "Bad
        /// Request" status code.
        /// </value>
        bool AllowMissing { get; }

        /// <summary>
        /// Gets the <see cref="Http.IFormCollection"/> property name, JSON path, or XPath used to read event names
        /// from the request body. Interpretation depends on the <see cref="IWebHookBodyTypeMetadataService.BodyType"/>
        /// of the receiver.
        /// </summary>
        /// <value>Should not return an empty string or <see langword="null"/>.</value>
        string BodyPropertyPath { get; }
    }
}
