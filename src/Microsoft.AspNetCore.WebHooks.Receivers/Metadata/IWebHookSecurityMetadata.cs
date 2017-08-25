// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Metadata describing the security aspects of a WebHook request. Implemented in a <see cref="IWebHookMetadata"/>
    /// service for receivers that do not include a specific <see cref="Filters.WebHookSecurityFilter"/> subclass or
    /// that respond to HTTP GET requests.
    /// </summary>
    public interface IWebHookSecurityMetadata : IWebHookMetadata, IWebHookReceiver
    {
        /// <summary>
        /// Gets an indication the <c>code</c> query parameter is required and should be compared with the configured
        /// secret key.
        /// </summary>
        bool VerifyCodeParameter { get; }

        /// <summary>
        /// Gets an indication the receiver should respond successfully to an HTTP GET request. The response may
        /// contain the value of the <see cref="WebHookGetRequest.ChallengeQueryParameterName"/> query parameter.
        /// </summary>
        bool ShortCircuitGetRequests { get; }

        /// <summary>
        /// Gets additional metadata about how to handle HTTP GET requests. Ignored if
        /// <see cref="ShortCircuitGetRequests"/> is <see langword="true"/>. If <see cref="ShortCircuitGetRequests"/>
        /// is <see langword="true"/> and this property is <see langword="null"/>, the receiver does no additional
        /// verification before responding to an HTTP GET request.
        /// </summary>
        WebHookGetRequest WebHookGetRequest { get; }
    }
}
