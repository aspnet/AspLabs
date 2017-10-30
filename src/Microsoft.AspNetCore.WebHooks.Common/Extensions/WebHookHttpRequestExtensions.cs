// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpRequest"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookHttpRequestExtensions
    {
        private static readonly MediaTypeHeaderValue ApplicationJsonMediaType
            = new MediaTypeHeaderValue("application/json");
        private static readonly MediaTypeHeaderValue ApplicationXmlMediaType
            = new MediaTypeHeaderValue("application/xml");
        private static readonly MediaTypeHeaderValue TextJsonMediaType = new MediaTypeHeaderValue("text/json");
        private static readonly MediaTypeHeaderValue TextXmlMediaType = new MediaTypeHeaderValue("text/xml");

        /// <summary>
        /// Determines whether the specified request contains JSON as indicated by a
        /// content type of either <c>application/json</c>, <c>text/json</c>, <c>application/xyz+json</c>,
        /// or <c>text/xyz+json</c>. The term <c>xyz</c> can for example be <c>hal</c> or some other
        /// JSON-derived media type.
        /// </summary>
        /// <returns>true if the specified request contains JSON content; otherwise, false.</returns>
        /// <param name="request">The <see cref="HttpRequest"/> to check.</param>
        public static bool IsJson(this HttpRequest request)
        {
            var contentType = request?.GetTypedHeaders()?.ContentType;
            if (contentType == null)
            {
                return false;
            }

            if (contentType.IsSubsetOf(ApplicationJsonMediaType) || contentType.IsSubsetOf(TextJsonMediaType))
            {
                return true;
            }

            var type = contentType.Type;
            if (type.Equals("application", StringComparison.OrdinalIgnoreCase) ||
                type.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                return contentType.SubType.EndsWith("+json", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified request contains XML as indicated by a
        /// content type of either <c>application/xml</c>, <c>text/xml</c>, <c>application/xyz+xml</c>,
        /// or <c>text/xyz+xml</c>. The term <c>xyz</c> can for example be <c>rdf</c> or some other
        /// XML-derived media type.
        /// </summary>
        /// <returns>true if the specified request contains XML content; otherwise, false.</returns>
        /// <param name="request">The <see cref="HttpRequest"/> to check.</param>
        public static bool IsXml(this HttpRequest request)
        {
            var contentType = request?.GetTypedHeaders()?.ContentType;
            if (contentType == null)
            {
                return false;
            }

            if (contentType.IsSubsetOf(ApplicationXmlMediaType) || contentType.IsSubsetOf(TextXmlMediaType))
            {
                return true;
            }

            var type = contentType.Type;
            if (type.Equals("application", StringComparison.OrdinalIgnoreCase) ||
                type.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                return contentType.SubType.EndsWith("+xml", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}