// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal
{
    internal static class JsonRequestHelpers
    {
        public const string JsonContentType = "application/json";
        public const string JsonContentTypeWithCharset = "application/json; charset=utf-8";

        public static bool HasJsonContentType(HttpRequest request, out StringSegment charset)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mt))
            {
                charset = StringSegment.Empty;
                return false;
            }

            // Matches application/json
            if (mt.MediaType.Equals(JsonContentType, StringComparison.OrdinalIgnoreCase))
            {
                charset = mt.Charset;
                return true;
            }

            // Matches +json, e.g. application/ld+json
            if (mt.Suffix.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                charset = mt.Charset;
                return true;
            }

            charset = StringSegment.Empty;
            return false;
        }

        public static (Stream stream, bool usesTranscodingStream) GetStream(Stream innerStream, Encoding? encoding)
        {
            if (encoding == null || encoding.CodePage == Encoding.UTF8.CodePage)
            {
                return (innerStream, false);
            }

            var stream = Encoding.CreateTranscodingStream(innerStream, encoding, Encoding.UTF8, leaveOpen: true);
            return (stream, true);
        }

        public static Encoding? GetEncodingFromCharset(StringSegment charset)
        {
            if (charset.Equals("utf-8", StringComparison.OrdinalIgnoreCase))
            {
                // This is an optimization for utf-8 that prevents the Substring caused by
                // charset.Value
                return Encoding.UTF8;
            }

            try
            {
                // charset.Value might be an invalid encoding name as in charset=invalid.
                return charset.HasValue ? Encoding.GetEncoding(charset.Value) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to read the request as JSON because the request content type charset '{charset}' is not a known encoding.", ex);
            }
        }
    }
}
