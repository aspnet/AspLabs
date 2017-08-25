// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;       // ??? BufferingHelper is pub-Internal.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// Base class for <see cref="IWebHookReceiver"/> and <see cref="Mvc.Filters.IResourceFilter"/> or
    /// <see cref="Mvc.Filters.IAsyncResourceFilter"/> implementations that verify request body content e.g. filters
    /// that verify signatures of request body content. Subclasses should have an
    /// <see cref="Mvc.Filters.IOrderedFilter.Order"/> equal to <see cref="WebHookSecurityFilter.Order"/>.
    /// </summary>
    public abstract class WebHookVerifyBodyContentFilter : WebHookSecurityFilter, IWebHookReceiver
    {
        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyBodyContentFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to initialize <see cref="WebHookSecurityFilter.Logger"/>.
        /// </param>
        /// <param name="receiverConfig">
        /// The <see cref="IWebHookReceiverConfig"/> used to initialize
        /// <see cref="WebHookSecurityFilter.Configuration"/> and <see cref="WebHookSecurityFilter.ReceiverConfig"/>.
        /// </param>
        protected WebHookVerifyBodyContentFilter(ILoggerFactory loggerFactory, IWebHookReceiverConfig receiverConfig)
            : base(loggerFactory, receiverConfig)
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
        /// Provides a time consistent comparison of two secrets in the form of two byte arrays.
        /// </summary>
        /// <param name="inputA">The first secret to compare.</param>
        /// <param name="inputB">The second secret to compare.</param>
        /// <returns><see langword="true"/> if the two secrets are equal; <see langword="false"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        protected internal static bool SecretEqual(byte[] inputA, byte[] inputB)
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
        /// Ensure we can read the <paramref name="request"/> body without messing up JSON etc. deserialization. Body
        /// will be read at least twice in most receivers.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to prepare.</param>
        /// <returns>A <see cref="Task"/> that on completion will have prepared the request body.</returns>
        public async Task PrepareRequestBody(HttpRequest request)
        {
            if (!request.Body.CanSeek)
            {
                BufferingHelper.EnableRewind(request);
                Debug.Assert(request.Body.CanSeek);

                await request.Body.DrainAsync(CancellationToken.None);
            }

            // Always start at the beginning.
            request.Body.Seek(0L, SeekOrigin.Begin);
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
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
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
                Logger.LogInformation(
                    400,
                    "Expecting exactly one '{HeaderName}' header field in the WebHook request but found " +
                    "{HeaderCount}. Please ensure the request contains exactly one '{HeaderName}' header field.",
                    headerName,
                    headersCount);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifySignature_BadHeader,
                    headerName,
                    headersCount);
                errorResult = WebHookResultUtilities.CreateErrorResult(message);

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
        /// A <see cref="Task"/> that on completion provides a <see cref="byte"/> array containing the SHA1 HMAC of the
        /// <paramref name="request"/>'s body.
        /// </returns>
        protected virtual async Task<byte[]> GetRequestBodyHash_SHA1(HttpRequest request, byte[] secret)
        {
            await PrepareRequestBody(request);

            using (var hasher = new HMACSHA1(secret))
            {
                try
                {
                    var hash = hasher.ComputeHash(request.Body);
                    return hash;
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
        protected virtual async Task<byte[]> GetRequestBodyHash_SHA256(HttpRequest request, byte[] secret)
        {
            await PrepareRequestBody(request);

            using (var hasher = new HMACSHA256(secret))
            {
                try
                {
                    var hash = hasher.ComputeHash(request.Body);
                    return hash;
                }
                finally
                {
                    // Reset Position because JsonInputFormatter et cetera always start from current position.
                    request.Body.Seek(0L, SeekOrigin.Begin);
                }
            }
        }

        /// <summary>
        /// Decode the given <paramref name="hexEncodedValue"/>.
        /// </summary>
        /// <param name="hexEncodedValue">The hex-encoded <see cref="string"/>.</param>
        /// <param name="signatureHeaderName">
        /// The name of the HTTP header containing the <paramref name="hexEncodedValue"/>.
        /// </param>
        /// <param name="errorResult">
        /// Set to <see langword="null"/> if decoding is successful. Otherwise, an <see cref="IActionResult"/> that
        /// when executed will produce a response containing details about the problem.
        /// </param>
        /// <returns>
        /// If successful, the <see cref="byte"/> array containing the decoded hash. <see langword="null"/> if any
        /// issues occur.
        /// </returns>
        protected virtual byte[] GetDecodedHash(
            string hexEncodedValue,
            string signatureHeaderName,
            out IActionResult errorResult)
        {
            try
            {
                var decodedHash = EncodingUtilities.FromHex(hexEncodedValue);
                errorResult = null;

                return decodedHash;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    401,
                    ex,
                    "The '{HeaderName}' header value is invalid. It must be a valid hex-encoded string.",
                    signatureHeaderName);
            }

            var message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.Security_BadHeaderEncoding,
                signatureHeaderName);
            errorResult = WebHookResultUtilities.CreateErrorResult(message);

            return null;
        }

        /// <summary>
        /// Returns a new <see cref="IActionResult"/> that when executed produces a response indicating that a
        /// request had an invalid signature and as a result could not be processed.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <param name="signatureHeaderName">The name of the HTTP header with invalid contents.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that when executed will produce a response with status code 400 "Bad
        /// Request" and containing details about the problem.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        protected virtual IActionResult CreateBadSignatureResult(string receiverName, string signatureHeaderName)
        {
            Logger.LogError(
                402,
                "The WebHook signature provided by the '{HeaderName}' header field does not match the value " +
                "expected by the '{ReceiverName}' receiver. WebHook request is invalid.",
                signatureHeaderName,
                receiverName);

            var message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.VerifySignature_BadSignature,
                signatureHeaderName,
                receiverName);
            var badSignature = WebHookResultUtilities.CreateErrorResult(message);

            return badSignature;
        }
    }
}