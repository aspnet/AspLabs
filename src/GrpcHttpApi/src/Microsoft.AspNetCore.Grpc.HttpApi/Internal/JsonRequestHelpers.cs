// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Gateway.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
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
                charset = default;
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

            charset = default;
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

        public static async Task SendErrorResponse(HttpResponse response, Encoding encoding, Status status, JsonSerializerOptions options)
        {
            var e = new Error
            {
                Error_ = status.Detail,
                Message = status.Detail,
                Code = (int)status.StatusCode
            };

            response.StatusCode = MapStatusCodeToHttpStatus(status.StatusCode);
            response.ContentType = MediaType.ReplaceEncoding("application/json", encoding);

            await WriteResponseMessage(response, encoding, e, options);
        }

        public static int MapStatusCodeToHttpStatus(StatusCode statusCode)
        {
            switch (statusCode)
            {
                case StatusCode.OK:
                    return StatusCodes.Status200OK;
                case StatusCode.Cancelled:
                    return StatusCodes.Status408RequestTimeout;
                case StatusCode.Unknown:
                    return StatusCodes.Status500InternalServerError;
                case StatusCode.InvalidArgument:
                    return StatusCodes.Status400BadRequest;
                case StatusCode.DeadlineExceeded:
                    return StatusCodes.Status504GatewayTimeout;
                case StatusCode.NotFound:
                    return StatusCodes.Status404NotFound;
                case StatusCode.AlreadyExists:
                    return StatusCodes.Status409Conflict;
                case StatusCode.PermissionDenied:
                    return StatusCodes.Status403Forbidden;
                case StatusCode.Unauthenticated:
                    return StatusCodes.Status401Unauthorized;
                case StatusCode.ResourceExhausted:
                    return StatusCodes.Status429TooManyRequests;
                case StatusCode.FailedPrecondition:
                    // Note, this deliberately doesn't translate to the similarly named '412 Precondition Failed' HTTP response status.
                    return StatusCodes.Status400BadRequest;
                case StatusCode.Aborted:
                    return StatusCodes.Status409Conflict;
                case StatusCode.OutOfRange:
                    return StatusCodes.Status400BadRequest;
                case StatusCode.Unimplemented:
                    return StatusCodes.Status501NotImplemented;
                case StatusCode.Internal:
                    return StatusCodes.Status500InternalServerError;
                case StatusCode.Unavailable:
                    return StatusCodes.Status503ServiceUnavailable;
                case StatusCode.DataLoss:
                    return StatusCodes.Status500InternalServerError;
            }

            return StatusCodes.Status500InternalServerError;
        }

        public static async Task WriteResponseMessage(HttpResponse response, Encoding encoding, object responseBody, JsonSerializerOptions options)
        {
            var (stream, usesTranscodingStream) = GetStream(response.Body, encoding);

            try
            {
                await JsonSerializer.SerializeAsync(stream, responseBody, options);
            }
            finally
            {
                if (usesTranscodingStream)
                {
                    await stream.DisposeAsync();
                }
            }
        }
    }
}
