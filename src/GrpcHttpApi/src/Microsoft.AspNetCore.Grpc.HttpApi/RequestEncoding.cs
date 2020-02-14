// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    // Logic from https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/Formatters/TextInputFormatter.cs
    internal static class RequestEncoding
    {
        /// <summary>
        /// Returns UTF8 Encoding without BOM and throws on invalid bytes.
        /// </summary>
        private static readonly Encoding UTF8EncodingWithoutBOM
            = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        /// <summary>
        /// Returns UTF16 Encoding which uses littleEndian byte order with BOM and throws on invalid bytes.
        /// </summary>
        private static readonly Encoding UTF16EncodingLittleEndian
            = new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);

        private static List<Encoding> SupportedEncodings { get; } = new List<Encoding>
        {
            UTF8EncodingWithoutBOM,
            UTF16EncodingLittleEndian
        };

        public static Encoding? SelectCharacterEncoding(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestContentType = request.ContentType;
            var requestMediaType = requestContentType == null ? default(MediaType) : new MediaType(requestContentType);
            if (requestMediaType.Charset.HasValue)
            {
                // Create Encoding based on requestMediaType.Charset to support charset aliases and custom Encoding
                // providers. Charset -> Encoding -> encoding.WebName chain canonicalizes the charset name.
                var requestEncoding = requestMediaType.Encoding;
                if (requestEncoding != null)
                {
                    for (var i = 0; i < SupportedEncodings.Count; i++)
                    {
                        if (string.Equals(
                            requestEncoding.WebName,
                            SupportedEncodings[i].WebName,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            return SupportedEncodings[i];
                        }
                    }
                }

                // The client specified an encoding in the content type header of the request
                // but we don't understand it. In this situation we don't try to pick any other encoding
                // from the list of supported encodings and read the body with that encoding.
                // Instead, we return null and that will translate later on into a 415 Unsupported Media Type
                // response.
                return null;
            }

            // We want to do our best effort to read the body of the request even in the
            // cases where the client doesn't send a content type header or sends a content
            // type header without encoding. For that reason we pick the first encoding of the
            // list of supported encodings and try to use that to read the body. This encoding
            // is UTF-8 by default in our formatters, which generally is a safe choice for the
            // encoding.
            return SupportedEncodings[0];
        }
    }
}
