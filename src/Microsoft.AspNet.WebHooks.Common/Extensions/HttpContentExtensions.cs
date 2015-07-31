// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Net.Http.Headers;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods for <see cref="MediaTypeHeaderValue"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpContentExtensions
    {
        /// <summary>
        /// Determines whether the specified content is JSON indicated by a 
        /// content type of either <c>application/json</c>, <c>text/json</c>, <c>application/xyz-json</c>,
        /// or <c>text/xyz-json</c>. The term <c>xyz</c> can for example be <c>hal</c> or some other 
        /// JSON derived media type.
        /// </summary>
        /// <returns>true if the specified content is JSON content; otherwise, false.</returns>
        /// <param name="content">The content to check.</param>
        public static bool IsJson(this HttpContent content)
        {
            if (content == null || content.Headers == null || content.Headers.ContentType == null || content.Headers.ContentType.MediaType == null)
            {
                return false;
            }

            string mediaType = content.Headers.ContentType.MediaType;
            return string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mediaType, "text/json", StringComparison.OrdinalIgnoreCase)
                || ((mediaType.StartsWith("application/", StringComparison.OrdinalIgnoreCase) || mediaType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)) 
                    && mediaType.EndsWith("-json", StringComparison.OrdinalIgnoreCase));
        }
    }
}
