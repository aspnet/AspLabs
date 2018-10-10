// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// Base class for <see cref="IWebHookReceiver"/> and <see cref="Mvc.Filters.IResourceFilter"/> or
    /// <see cref="Mvc.Filters.IAsyncResourceFilter"/> implementations that verify request body content e.g. filters
    /// that verify signatures of request body content. Subclasses by default have an
    /// <see cref="Mvc.Filters.IOrderedFilter.Order"/> equal to <see cref="WebHookSecurityFilter.Order"/>.
    /// </summary>
    public abstract class WebHookVerifySignatureFilter : WebHookSecurityFilter, IWebHookReceiver
    {
        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifySignatureFilter"/> instance.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> used to initialize <see cref="WebHookSecurityFilter.Configuration"/>.
        /// </param>
        /// <param name="hostingEnvironment">
        /// The <see cref="IHostingEnvironment" /> used to initialize
        /// <see cref="WebHookSecurityFilter.HostingEnvironment"/>.
        /// </param>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to initialize <see cref="WebHookSecurityFilter.Logger"/>.
        /// </param>
        protected WebHookVerifySignatureFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory)
            : base(configuration, hostingEnvironment, loggerFactory)
        {
        }

        /// <inheritdoc />
        public abstract string ReceiverName { get; }

        /// <inheritdoc />
        public bool IsApplicable(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            return string.Equals(ReceiverName, receiverName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts a Base64-encoded string to a <see cref="T:byte[]"/>.
        /// </summary>
        /// <param name="content">THe Base64-encoded string to convert.</param>
        /// <param name="signatureHeaderName">
        /// The name of the HTTP header containing <paramref name="content"/>.
        /// </param>
        /// <returns>The converted <see cref="T:byte[]"/>. <see langword="null"/> if conversion fails.</returns>
        protected byte[] FromBase64(string content, string signatureHeaderName)
        {
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<byte>();
            }

            try
            {
                return Base64UrlTextEncoder.Decode(content);
            }
            catch (FormatException exception)
            {
                Logger.LogWarning(
                    400,
                    exception,
                    "The '{HeaderName}' header value is invalid. The '{ReceiverName}' receiver requires a valid " +
                    "hex-encoded string.",
                    signatureHeaderName,
                    ReceiverName);

                return null;
            }
        }

        /// <summary>
        /// Converts a hex-encoded string to a <see cref="T:byte[]"/>.
        /// </summary>
        /// <param name="content">THe hex-encoded string to convert.</param>
        /// <param name="signatureHeaderName">
        /// The name of the HTTP header containing <paramref name="content"/>.
        /// </param>
        /// <returns>The converted <see cref="T:byte[]"/>. <see langword="null"/> if conversion fails.</returns>
        protected byte[] FromHex(string content, string signatureHeaderName)
        {
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<byte>();
            }

            try
            {
                var data = new byte[content.Length / 2];
                var input = 0;
                for (var output = 0; output < data.Length; output++)
                {
                    data[output] = Convert.ToByte(new string(new char[2] { content[input++], content[input++] }), 16);
                }

                if (input != content.Length)
                {
                    Logger.LogWarning(
                        401,
                        "The '{HeaderName}' header value is invalid. The '{ReceiverName}' receiver requires a valid " +
                        "hex-encoded string.",
                        signatureHeaderName,
                        ReceiverName);

                    return null;
                }

                return data;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is FormatException)
            {
                // FormatException is most likely. ToByte throws an ArgumentException when e.g. content contains a
                // minus sign ('-').
                Logger.LogWarning(
                    402,
                    exception,
                    "The '{HeaderName}' header value is invalid. The '{ReceiverName}' receiver requires a valid " +
                    "hex-encoded string.",
                    signatureHeaderName,
                    ReceiverName);

                return null;
            }
        }

        /// <summary>
        /// Provides a time consistent comparison of two secrets in the form of two byte arrays.
        /// </summary>
        /// <param name="inputA">The first secret to compare.</param>
        /// <param name="inputB">The second secret to compare.</param>
        /// <returns><see langword="true"/> if the two secrets are equal; <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        protected static bool SecretEqual(byte[] inputA, byte[] inputB)
        {
            if (ReferenceEquals(inputA, inputB))
            {
                return true;
            }

            if (inputA == null || inputB == null || inputA.Length != inputB.Length)
            {
                return false;
            }

            var areSame = true;
            for (var i = 0; i < inputA.Length; i++)
            {
                areSame &= inputA[i] == inputB[i];
            }

            return areSame;
        }

        /// <summary>
        /// Gets the value of a given HTTP request <paramref name="headerName"/>. If the field is either not present in
        /// the <paramref name="request"/> or has more than one value then an error is generated and returned in
        /// <paramref name="errorResult"/>.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="headerName">The name of the HTTP request header to look up.</param>
        /// <param name="errorResult">
        /// Set to <see langword="null"/> in the success case. When a check fails, an <see cref="IActionResult"/> that
        /// when executed will produce a response containing details about the problem.
        /// </param>
        /// <returns>The signature header; <see langword="null"/> if this cannot be found.</returns>
        protected virtual string GetRequestHeader(
            HttpRequest request,
            string headerName,
            out IActionResult errorResult)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (headerName == null)
            {
                throw new ArgumentNullException(nameof(headerName));
            }

            if (!request.Headers.TryGetValue(headerName, out var headers) || headers.Count != 1)
            {
                var headersCount = headers.Count;
                Logger.LogWarning(
                    403,
                    "Expecting exactly one '{HeaderName}' header field in the WebHook request but found " +
                    "{HeaderCount}. Ensure the request contains exactly one '{HeaderName}' header field.",
                    headerName,
                    headersCount,
                    headerName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifySignature_BadHeader,
                    headerName,
                    headersCount);
                errorResult = new BadRequestObjectResult(message);

                return null;
            }

            errorResult = null;

            return headers;
        }

        /// <summary>
        /// Returns the SHA1 HMAC of the given <paramref name="request"/>'s body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="secret">The key data used to initialize the <see cref="HMACSHA1"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <see cref="byte"/> array containing the SHA1 HMAC of
        /// the <paramref name="request"/>'s body.
        /// </returns>
        protected Task<byte[]> ComputeRequestBodySha1HashAsync(HttpRequest request, byte[] secret)
        {
            return ComputeRequestBodySha1HashAsync(request, secret, prefix: null);
        }

        /// <summary>
        /// Returns the SHA1 HMAC of the given <paramref name="prefix"/> followed by the given
        /// <paramref name="request"/>'s body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="secret">The key data used to initialize the <see cref="HMACSHA1"/>.</param>
        /// <param name="prefix">
        /// If non-<see langword="null"/> and non-empty, additional <c>byte</c>s to include in the hashed content
        /// before the <paramref name="request"/>'s body.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <see cref="byte"/> array containing the SHA1 HMAC of
        /// the <paramref name="prefix"/> followed by the <paramref name="request"/>'s body.
        /// </returns>
        protected Task<byte[]> ComputeRequestBodySha1HashAsync(HttpRequest request, byte[] secret, byte[] prefix)
        {
            return ComputeRequestBodySha1HashAsync(request, secret, prefix, suffix: null);
        }

        /// <summary>
        /// Returns the SHA1 HMAC of the given <paramref name="prefix"/>, the given <paramref name="request"/>'s
        /// body, and the given <paramref name="suffix"/> (in that order).
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="secret">The key data used to initialize the <see cref="HMACSHA1"/>.</param>
        /// <param name="prefix">
        /// If non-<see langword="null"/> and non-empty, additional <c>byte</c>s to include in the hashed content
        /// before the <paramref name="request"/>'s body.
        /// </param>
        /// <param name="suffix">
        /// If non-<see langword="null"/> and non-empty, additional <c>byte</c>s to include in the hashed content
        /// after the <paramref name="request"/>'s body.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <see cref="byte"/> array containing the SHA1 HMAC of
        /// the <paramref name="prefix"/>, the <paramref name="request"/>'s body, and the <paramref name="suffix"/>
        /// (in that order).
        /// </returns>
        protected virtual async Task<byte[]> ComputeRequestBodySha1HashAsync(
            HttpRequest request,
            byte[] secret,
            byte[] prefix,
            byte[] suffix)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }
            if (secret.Length == 0)
            {
                throw new ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(secret));
            }

            await WebHookHttpRequestUtilities.PrepareRequestBody(request);

            using (var hasher = new HMACSHA1(secret))
            {
                try
                {
                    if (prefix != null && prefix.Length > 0)
                    {
                        hasher.TransformBlock(
                            prefix,
                            inputOffset: 0,
                            inputCount: prefix.Length,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    // Split body into 4K chunks.
                    var buffer = new byte[4096];
                    var inputStream = request.Body;
                    int bytesRead;
                    while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        hasher.TransformBlock(
                            buffer,
                            inputOffset: 0,
                            inputCount: bytesRead,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    if (suffix != null && suffix.Length > 0)
                    {
                        hasher.TransformBlock(
                            suffix,
                            inputOffset: 0,
                            inputCount: suffix.Length,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    hasher.TransformFinalBlock(Array.Empty<byte>(), inputOffset: 0, inputCount: 0);

                    return hasher.Hash;
                }
                finally
                {
                    // Reset Position because JsonInputFormatter et cetera always start from current position.
                    request.Body.Seek(0L, SeekOrigin.Begin);
                }
            }
        }

        /// <summary>
        /// Returns the SHA256 HMAC of the given <paramref name="request"/>'s body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="secret">The key data used to initialize the <see cref="HMACSHA256"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <see cref="byte"/> array containing the SHA256 HMAC of
        /// the <paramref name="request"/>'s body.
        /// </returns>
        protected Task<byte[]> ComputeRequestBodySha256HashAsync(HttpRequest request, byte[] secret)
        {
            return ComputeRequestBodySha256HashAsync(request, secret, prefix: null);
        }

        /// <summary>
        /// Returns the SHA256 HMAC of the given <paramref name="prefix"/> followed by the given
        /// <paramref name="request"/>'s body.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="secret">The key data used to initialize the <see cref="HMACSHA256"/>.</param>
        /// <param name="prefix">
        /// If non-<see langword="null"/> and non-empty, additional <c>byte</c>s to include in the hashed content
        /// before the <paramref name="request"/>'s body.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <see cref="byte"/> array containing the SHA256 HMAC of
        /// the <paramref name="prefix"/> followed by the <paramref name="request"/>'s body.
        /// </returns>
        protected Task<byte[]> ComputeRequestBodySha256HashAsync(HttpRequest request, byte[] secret, byte[] prefix)
        {
            return ComputeRequestBodySha256HashAsync(request, secret, prefix, suffix: null);
        }

        /// <summary>
        /// Returns the SHA256 HMAC of the given <paramref name="prefix"/>, the given <paramref name="request"/>'s
        /// body, and the given <paramref name="suffix"/> (in that order).
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="secret">The key data used to initialize the <see cref="HMACSHA256"/>.</param>
        /// <param name="prefix">
        /// If non-<see langword="null"/> and non-empty, additional <c>byte</c>s to include in the hashed content
        /// before the <paramref name="request"/>'s body.
        /// </param>
        /// <param name="suffix">
        /// If non-<see langword="null"/> and non-empty, additional <c>byte</c>s to include in the hashed content
        /// after the <paramref name="request"/>'s body.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <see cref="byte"/> array containing the SHA256 HMAC of
        /// the <paramref name="prefix"/>, the <paramref name="request"/>'s body, and the <paramref name="suffix"/>
        /// (in that order).
        /// </returns>
        protected virtual async Task<byte[]> ComputeRequestBodySha256HashAsync(
            HttpRequest request,
            byte[] secret,
            byte[] prefix,
            byte[] suffix)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (secret == null)
            {
                throw new ArgumentNullException(nameof(secret));
            }
            if (secret.Length == 0)
            {
                throw new ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(secret));
            }

            await WebHookHttpRequestUtilities.PrepareRequestBody(request);

            using (var hasher = new HMACSHA256(secret))
            {
                try
                {
                    if (prefix != null && prefix.Length > 0)
                    {
                        hasher.TransformBlock(
                            inputBuffer: prefix,
                            inputOffset: 0,
                            inputCount: prefix.Length,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    // Split body into 4K chunks.
                    var buffer = new byte[4096];
                    var inputStream = request.Body;
                    int bytesRead;
                    while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        hasher.TransformBlock(
                            buffer,
                            inputOffset: 0,
                            inputCount: bytesRead,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    if (suffix != null && suffix.Length > 0)
                    {
                        hasher.TransformBlock(
                            suffix,
                            inputOffset: 0,
                            inputCount: suffix.Length,
                            outputBuffer: null,
                            outputOffset: 0);
                    }

                    hasher.TransformFinalBlock(Array.Empty<byte>(), inputOffset: 0, inputCount: 0);

                    return hasher.Hash;
                }
                finally
                {
                    // Reset Position because JsonInputFormatter et cetera always start from current position.
                    request.Body.Seek(0L, SeekOrigin.Begin);
                }
            }
        }

        /// <summary>
        /// Returns a new <see cref="IActionResult"/> that when executed produces a response indicating the request
        /// had a signature header containing an invalid (non-Base64-encoded) hash value.
        /// </summary>
        /// <param name="signatureHeaderName">The name of the HTTP header with invalid content.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that when executed will produce a response with status code 400 "Bad
        /// Request" and containing details about the problem.
        /// </returns>
        protected virtual IActionResult CreateBadBase64EncodingResult(string signatureHeaderName)
        {
            if (signatureHeaderName == null)
            {
                throw new ArgumentNullException(nameof(signatureHeaderName));
            }

            var message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.VerifySignature_BadBase64Encoding,
                signatureHeaderName,
                ReceiverName);

            return new BadRequestObjectResult(message);
        }

        /// <summary>
        /// Returns a new <see cref="IActionResult"/> that when executed produces a response indicating the request
        /// had a signature header containing an invalid (non-hex-encoded) hash value.
        /// </summary>
        /// <param name="signatureHeaderName">The name of the HTTP header with invalid content.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that when executed will produce a response with status code 400 "Bad
        /// Request" and containing details about the problem.
        /// </returns>
        protected virtual IActionResult CreateBadHexEncodingResult(string signatureHeaderName)
        {
            if (signatureHeaderName == null)
            {
                throw new ArgumentNullException(nameof(signatureHeaderName));
            }

            var message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.VerifySignature_BadHexEncoding,
                signatureHeaderName,
                ReceiverName);

            return new BadRequestObjectResult(message);
        }

        /// <summary>
        /// Returns a new <see cref="IActionResult"/> that when executed produces a response indicating the request
        /// invalid signature (an unexpected hash value) and as a result could not be processed. Also logs about the
        /// problem.
        /// </summary>
        /// <param name="signatureHeaderName">The name of the HTTP header with invalid content.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that when executed will produce a response with status code 400 "Bad
        /// Request" and containing details about the problem.
        /// </returns>
        protected virtual IActionResult CreateBadSignatureResult(string signatureHeaderName)
        {
            if (signatureHeaderName == null)
            {
                throw new ArgumentNullException(nameof(signatureHeaderName));
            }

            Logger.LogWarning(
                404,
                "The WebHook signature provided by the '{HeaderName}' header field does not match the value " +
                "expected by the '{ReceiverName}' receiver. WebHook request is invalid.",
                signatureHeaderName,
                ReceiverName);

            var message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.VerifySignature_BadSignature,
                signatureHeaderName,
                ReceiverName);

            return new BadRequestObjectResult(message);
        }
    }
}
