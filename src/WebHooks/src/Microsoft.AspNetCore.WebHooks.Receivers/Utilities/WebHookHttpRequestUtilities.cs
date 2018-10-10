// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Microsoft.AspNetCore.WebHooks.Utilities
{
    /// <summary>
    /// Utility methods related to <see cref="HttpRequest"/> instances.
    /// </summary>
    internal static class WebHookHttpRequestUtilities
    {
        /// <summary>
        /// Ensure we can read the <paramref name="request"/> body without messing up JSON etc. deserialization. Body
        /// will be read at least twice in most WebHook receivers.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to prepare.</param>
        /// <returns>A <see cref="Task"/> that on completion will have prepared the request body.</returns>
        public static async Task PrepareRequestBody(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.Body.CanSeek)
            {
                request.EnableBuffering();
                Debug.Assert(request.Body.CanSeek);

                await request.Body.DrainAsync(CancellationToken.None);
            }

            // Always start at the beginning.
            request.Body.Seek(0L, SeekOrigin.Begin);
        }
    }
}
