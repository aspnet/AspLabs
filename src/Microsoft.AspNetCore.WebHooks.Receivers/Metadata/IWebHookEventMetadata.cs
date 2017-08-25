// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Metadata describing the source of a WebHook <see cref="string"/> or <c>string[]</c> action parameter named
    /// <c>action</c> or <c>@event</c> and (optionally) the event name for a ping request. Implemented in a
    /// <see cref="IWebHookMetadata"/> service for receivers that do not place event information in the request body.
    /// </summary>
    public interface IWebHookEventMetadata : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Gets the constant event name for this receiver. Used as a fallback when <see cref="HeaderName"/> and
        /// <see cref="QueryParameterName"/> are <see langword="null"/> or do not match the request.
        /// </summary>
        /// <value>Must not return an empty string.</value>
        string ConstantValue { get; }

        /// <summary>
        /// Gets the name of the header containing event name(s) for this receiver.
        /// </summary>
        /// <value>Must not return an empty string.</value>
        string HeaderName { get; }

        /// <summary>
        /// Gets the name of the ping event for this receiver.
        /// </summary>
        /// <value>
        /// Must not return an empty string. Should be <see langword="null"/> if the receiver uses GET or HEAD for ping
        /// requests.
        /// </value>
        string PingEventName { get; }

        /// <summary>
        /// Gets the name of the query parameter containing event name(s) for this receiver.
        /// </summary>
        /// <value>Must not return an empty string.</value>
        string QueryParameterName { get; }
    }
}
