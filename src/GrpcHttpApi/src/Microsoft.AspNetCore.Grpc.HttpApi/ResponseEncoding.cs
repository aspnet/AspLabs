// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using StringWithQualityHeaderValue = Microsoft.Net.Http.Headers.StringWithQualityHeaderValue;

namespace Microsoft.AspNetCore.Grpc.HttpApi
{
    // Logic from https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Formatters/TextOutputFormatter.cs
    internal static class ResponseEncoding
    {
        private static List<Encoding> SupportedEncodings { get; } = new List<Encoding>
        {
            Encoding.UTF8,
            Encoding.Unicode
        };

        public static Encoding SelectCharacterEncoding(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var acceptCharsetHeaderValues = GetAcceptCharsetHeaderValues(request);
            var encoding = MatchAcceptCharacterEncoding(acceptCharsetHeaderValues);
            if (encoding != null)
            {
                return encoding;
            }

            if (!string.IsNullOrEmpty(request.ContentType))
            {
                var parsedContentType = new MediaType(request.ContentType);
                var contentTypeCharset = parsedContentType.Charset;
                if (contentTypeCharset.HasValue)
                {
                    for (var i = 0; i < SupportedEncodings.Count; i++)
                    {
                        var supportedEncoding = SupportedEncodings[i];
                        if (contentTypeCharset.Equals(supportedEncoding.WebName, StringComparison.OrdinalIgnoreCase))
                        {
                            // This is supported.
                            return SupportedEncodings[i];
                        }
                    }
                }
            }

            return SupportedEncodings[0];
        }

        private static Encoding? MatchAcceptCharacterEncoding(IList<StringWithQualityHeaderValue> acceptCharsetHeaders)
        {
            if (acceptCharsetHeaders != null && acceptCharsetHeaders.Count > 0)
            {
                var acceptValues = Sort(acceptCharsetHeaders);
                for (var i = 0; i < acceptValues.Count; i++)
                {
                    var charset = acceptValues[i].Value;
                    if (!StringSegment.IsNullOrEmpty(charset))
                    {
                        for (var j = 0; j < SupportedEncodings.Count; j++)
                        {
                            var encoding = SupportedEncodings[j];
                            if (charset.Equals(encoding.WebName, StringComparison.OrdinalIgnoreCase) ||
                                charset.Equals("*", StringComparison.Ordinal))
                            {
                                return encoding;
                            }
                        }
                    }
                }
            }

            return null;
        }

        // There's no allocation-free way to sort an IList and we may have to filter anyway,
        // so we're going to have to live with the copy + insertion sort.
        private static IList<StringWithQualityHeaderValue> Sort(IList<StringWithQualityHeaderValue> values)
        {
            var sortNeeded = false;

            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value.Quality == HeaderQuality.NoMatch)
                {
                    // Exclude this one
                }
                else if (value.Quality != null)
                {
                    sortNeeded = true;
                }
            }

            if (!sortNeeded)
            {
                return values;
            }

            var sorted = new List<StringWithQualityHeaderValue>();
            for (var i = 0; i < values.Count; i++)
            {
                var value = values[i];
                if (value.Quality == HeaderQuality.NoMatch)
                {
                    // Exclude this one
                }
                else
                {
                    // Doing an insertion sort.
                    var position = sorted.BinarySearch(value, StringWithQualityHeaderValueComparer.QualityComparer);
                    if (position >= 0)
                    {
                        sorted.Insert(position + 1, value);
                    }
                    else
                    {
                        sorted.Insert(~position, value);
                    }
                }
            }

            // We want a descending sort, but BinarySearch does ascending
            sorted.Reverse();
            return sorted;
        }

        private static IList<StringWithQualityHeaderValue> GetAcceptCharsetHeaderValues(HttpRequest request)
        {
            if (StringWithQualityHeaderValue.TryParseList((IList<string>)request.Headers[HeaderNames.AcceptCharset], out var result))
            {
                return result;
            }

            return Array.Empty<StringWithQualityHeaderValue>();
        }
    }
}
