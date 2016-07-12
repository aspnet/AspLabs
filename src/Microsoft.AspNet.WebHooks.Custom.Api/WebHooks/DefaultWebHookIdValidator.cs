// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides a default implementation of <see cref="IWebHookIdValidator"/> which simply deletes any Id provided 
    /// by a client and instead forces a valid Id to be created on server side.
    /// </summary>
    public class DefaultWebHookIdValidator : IWebHookIdValidator
    {
        /// <inheritdoc/>
        public Task ValidateIdAsync(HttpRequestMessage request, WebHook webHook)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            // Ensure we have a normalized ID for the WebHook
            webHook.Id = null;
            return Task.FromResult(true);
        }
    }
}
