// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// <para>
    /// Metadata indicating the receiver should confirm an appropriate secret key is configured and respond
    /// successfully to an HTTP GET or HEAD request. When <see cref="ChallengeQueryParameterName"/> is
    /// non-<see langword="null"/>, also indicates the response to an HTTP GET request should contain the value of the
    /// <see cref="ChallengeQueryParameterName"/> query parameter. Implemented in a <see cref="IWebHookMetadata"/>
    /// service for receivers that accept GET or HEAD requests.
    /// </para>
    /// <para>
    /// <see cref="Filters.WebHookGetHeadRequestFilter"/> performs verifications and short-circuits GET or HEAD
    /// requests based on this metadata. Requests for receivers that do not implement
    /// <see cref="IWebHookGetHeadRequestMetadata"/> reach the <see cref="Filters.WebHookVerifyMethodFilter"/>.
    /// <see cref="Filters.WebHookVerifyMethodFilter"/> returns a response with status code 405, "Method Not Allowed"
    /// for all non-POST requests.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The secret key confirmation is particularly important for receivers secured using the request body (for
    /// example, receivers that verify signatures).
    /// </remarks>
    public interface IWebHookGetHeadRequestMetadata : IWebHookMetadata, IWebHookReceiver
    {

        /// <summary>
        /// Gets an indication the receiver should short-circuit HTTP HEAD requests instead of HTTP GET requests.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, the receiver should short-circuit HTTP HEAD requests. Otherwise, it should
        /// short-circuit HTTP GET requests.
        /// </value>
        bool AllowHeadRequests { get; }

        /// <summary>
        /// Gets the name of a query parameter containing a value to include in the response to an HTTP GET request.
        /// If non-<see langword="null"/>, the request must contain this query parameter and the receiver responds
        /// with its value in the body. Otherwise, the receiver responds to an HTTP GET request with an empty
        /// response body. The receiver ignores this parameter for HTTP POST requests.
        /// </summary>
        /// <value>Should not return an empty string.</value>
        string ChallengeQueryParameterName { get; }

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver. Used to confirm the secret key is
        /// property configured before responding to an HTTP GET or HEAD request.
        /// </summary>
        int SecretKeyMinLength { get; }
    }
}
