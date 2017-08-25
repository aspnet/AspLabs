// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Metadata describing handling of an HTTP GET request.
    /// </summary>
    public class WebHookGetRequest
    {
        /// <summary>
        /// Instantiates a new <see cref="WebHookGetRequest"/> instance.
        /// </summary>
        /// <param name="challengeQueryParameterName">
        /// The name of a query parameter containing a value to include in the response to an HTTP GET request.
        /// </param>
        /// <param name="secretKeyMinLength">The minimum length of the secret key configured for this receiver.</param>
        /// <param name="secretKeyMaxLength">The maximum length of the secret key configured for this receiver.</param>
        public WebHookGetRequest(string challengeQueryParameterName, int secretKeyMinLength, int secretKeyMaxLength)
        {
            if (string.IsNullOrEmpty(challengeQueryParameterName))
            {
                throw new ArgumentException(
                    Resources.General_ArgumentCannotBeNullOrEmpty,
                    nameof(challengeQueryParameterName));
            }

            ChallengeQueryParameterName = challengeQueryParameterName;
            SecretKeyMinLength = secretKeyMinLength;
            SecretKeyMaxLength = secretKeyMaxLength;
        }

        /// <summary>
        /// Gets the name of a query parameter containing a value to include in the response to an HTTP GET request.
        /// </summary>
        public string ChallengeQueryParameterName { get; }

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver. Used to confirm the secret key is
        /// property configured before responding to an HTTP GET request.
        /// </summary>
        public int SecretKeyMinLength { get; }

        /// <summary>
        /// Gets the maximum length of the secret key configured for this receiver. Used to confirm the secret key is
        /// property configured before responding to an HTTP GET request.
        /// </summary>
        public int SecretKeyMaxLength { get; }
    }
}
