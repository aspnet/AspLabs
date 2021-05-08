// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// <see cref="MediaTypeFormatter"/> class to handle Json.
    /// </summary>
    public class SystemTextJsonMediaTypeFormatter : MediaTypeFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonMediaTypeFormatter"/> class.
        /// </summary>
        public SystemTextJsonMediaTypeFormatter()
        {
            // Set default supported media types
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/json"));

            // Set default supported character encodings
            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }

        /// <summary>
        /// Gets or sets the <see cref="JsonSerializerOptions" /> used to format data. Configured using <see cref="JsonSerializerDefaults.Web" />.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
            => ReadFromStreamAsync(type, readStream, content, formatterLogger, default);

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (readStream == null)
            {
                throw new ArgumentNullException("readStream");
            }

            HttpContentHeaders contentHeaders = content == null ? null : content.Headers;

            // If content length is 0 then return default value for this type
            if (contentHeaders != null && contentHeaders.ContentLength == 0)
            {
                return GetDefaultValueForType(type);
            }

            // Get the character encoding for the content
            // Never non-null since SelectCharacterEncoding() throws in error / not found scenarios
            Encoding effectiveEncoding = SelectCharacterEncoding(contentHeaders);

            Stream transcodingStream = null;
            if (effectiveEncoding.CodePage != Encoding.UTF8.CodePage)
            {
#if NET5_0_OR_GREATER
                transcodingStream = Encoding.CreateTranscodingStream(readStream, Encoding.UTF8, effectiveEncoding, leaveOpen: true);
#else
                throw new NotSupportedException("Using non-UTF8 encoding is not supported.");
#endif
            }

            try
            {
                var result = await JsonSerializer.DeserializeAsync(transcodingStream ?? readStream, type, JsonSerializerOptions, cancellationToken);
                return result;
            }
            finally
            {
#if NET5_0_OR_GREATER
                await (transcodingStream?.DisposeAsync() ?? default);
#else
                transcodingStream?.Dispose();
#endif
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
            => WriteToStreamAsync(type, value, writeStream, content, transportContext, default);

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (writeStream == null)
            {
                throw new ArgumentNullException("writeStream");
            }

            Encoding effectiveEncoding = SelectCharacterEncoding(content == null ? null : content.Headers);
            Stream transcodingStream = null;
            if (effectiveEncoding.CodePage != Encoding.UTF8.CodePage)
            {
#if NET5_0_OR_GREATER
                transcodingStream = Encoding.CreateTranscodingStream(writeStream, Encoding.UTF8, effectiveEncoding, leaveOpen: true);
#else
                throw new NotSupportedException("Using non-UTF8 encoding is not supported.");
#endif
            }

            try
            {
                await JsonSerializer.SerializeAsync(transcodingStream ?? writeStream, value, type, JsonSerializerOptions, cancellationToken);
            }
            finally
            {
#if NET5_0_OR_GREATER
                await (transcodingStream?.DisposeAsync() ?? default);
#else
                transcodingStream?.Dispose();
#endif
            }
        }

        public override bool CanReadType(Type type) => true;

        public override bool CanWriteType(Type type) => true;
    }
}
