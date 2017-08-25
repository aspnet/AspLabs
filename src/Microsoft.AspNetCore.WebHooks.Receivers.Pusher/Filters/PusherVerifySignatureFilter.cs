// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> that verifies the Pusher signature header. Confirms the header exists, reads
    /// Body bytes, and compares the hashes.
    /// </summary>
    public class PusherVerifySignatureFilter : WebHookVerifyBodyContentFilter, IAsyncResourceFilter
    {
        // Character that appears between key pairs.
        private static readonly char[] BetweenPairSeparators = new[] { ';' };

        // Character that appears between the application key and secret key in a pair.
        private static readonly char[] PairSeparators = new[] { '_' };

        // Map from receiver id to configured secret key lookup table. Receiver configuration uses Ordinal dictionaries
        // but lowercases all strings before comparisons.
        private readonly ConcurrentDictionary<string, IDictionary<string, string>> _lookupTables =
            new ConcurrentDictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Instantiates a new <see cref="PusherVerifySignatureFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to initialize <see cref="WebHookSecurityFilter.Logger"/>.
        /// </param>
        /// <param name="receiverConfig">
        /// The <see cref="IWebHookReceiverConfig"/> used to initialize
        /// <see cref="WebHookSecurityFilter.Configuration"/> and <see cref="WebHookSecurityFilter.ReceiverConfig"/>.
        /// </param>
        public PusherVerifySignatureFilter(ILoggerFactory loggerFactory, IWebHookReceiverConfig receiverConfig)
            : base(loggerFactory, receiverConfig)
        {
        }

        /// <inheritdoc />
        public override string ReceiverName => PusherConstants.ReceiverName;

        /// <inheritdoc />
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var routeData = context.RouteData;
            var request = context.HttpContext.Request;
            if (routeData.TryGetReceiverName(out var receiverName) &&
                IsApplicable(receiverName) &&
                HttpMethods.IsPost(request.Method))
            {
                // 1. Get the expected hash from the signature headers.
                var header = GetRequestHeader(request, PusherConstants.SignatureHeaderName, out var errorResult);
                if (errorResult != null)
                {
                    context.Result = errorResult;
                    return;
                }

                var expectedHash = GetDecodedHash(header, PusherConstants.SignatureHeaderName, out errorResult);
                if (errorResult != null)
                {
                    context.Result = errorResult;
                    return;
                }

                // 2. Get the configured secret key.
                var lookupTable = await GetSecretLookupTable(request, routeData);
                if (lookupTable == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var applicationKey = GetRequestHeader(request, PusherConstants.SignatureKeyHeaderName, out errorResult);
                if (errorResult != null)
                {
                    context.Result = errorResult;
                    return;
                }

                if (!lookupTable.TryGetValue(applicationKey, out var secretKey))
                {
                    Logger.LogError(
                        0,
                        "The '{HeaderName}' header value of '{HeaderValue}' is not recognized as a valid " +
                        "application key. Please ensure the correct application key / secret key pairs have " +
                        "been configured.",
                        PusherConstants.SignatureKeyHeaderName,
                        applicationKey);

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.SignatureFilter_SecretNotFound,
                        PusherConstants.SignatureKeyHeaderName,
                        applicationKey);

                    context.Result = WebHookResultUtilities.CreateErrorResult(message);
                    return;
                }

                var secret = Encoding.UTF8.GetBytes(secretKey);

                // 3. Get the actual hash of the request body.
                var actualHash = await GetRequestBodyHash_SHA256(request, secret);

                // 4. Verify that the actual hash matches the expected hash.
                if (!SecretEqual(expectedHash, actualHash))
                {
                    // Log about the issue and short-circuit remainder of the pipeline.
                    errorResult = CreateBadSignatureResult(receiverName, PusherConstants.SignatureHeaderName);

                    context.Result = errorResult;
                    return;
                }
            }

            await next();
        }

        /// <summary>
        /// Gets the set of tuples mapping application keys to secret keys. The secret keys are used to verify the
        /// signature of an incoming Pusher WebHook request.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="routeData">
        /// The <see cref="RouteData"/> for this request. A (potentially empty) ID value in this data allows
        /// <see cref="PusherVerifySignatureFilter"/> to support multiple senders with individual configurations.
        /// </param>
        /// <returns></returns>
        protected virtual async Task<IDictionary<string, string>> GetSecretLookupTable(
            HttpRequest request,
            RouteData routeData)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            // 1. Check if we've already found the configured lookup table for this id.
            routeData.TryGetReceiverId(out var id);
            id = id ?? string.Empty;
            if (_lookupTables.TryGetValue(id, out var lookupTable))
            {
                // Success.
                return lookupTable;
            }

            // 2. Get the configuration value.
            var secretKeyPairs = await GetReceiverConfig(
                request,
                routeData,
                ReceiverName,
                PusherConstants.SecretKeyMinLength,
                PusherConstants.SecretKeyMaxLength);
            if (secretKeyPairs == null)
            {
                // Missing a configuration value for this id.
                return null;
            }

            // 3. Parse the configuration value as application key / secret key pairs.;
            lookupTable = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var secretKeyPair in new TrimmingTokenizer(secretKeyPairs, BetweenPairSeparators))
            {
                var partTokenizer = new TrimmingTokenizer(secretKeyPair, PairSeparators);
                if (partTokenizer.Count != 2)
                {
                    // Corrupted configuration value.
                    Logger.LogCritical(
                        1,
                        "Could not find a valid configuration for the '{ReceiverName}' WebHook receiver and " +
                        "instance '{Id}'. The configuration value must be a comma-separated list of segments, each " +
                        "of the form '<appKey1>_<secretKey1>; <appKey2>_<secretKey2>'.",
                        ReceiverName,
                        id);

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.SignatureFilter_BadSecret,
                        ReceiverName,
                        id);
                    throw new InvalidOperationException(Resources.SignatureFilter_BadSecret);
                }

                // Empty or duplicate application keys are fine; will be ignored during lookups or merged when added to
                // the table (keeping the last value).
                var enumerator = partTokenizer.GetEnumerator();
                enumerator.MoveNext();
                var applicationKey = enumerator.Current.Value;
                enumerator.MoveNext();
                var secretKey = enumerator.Current.Value;
                lookupTable[applicationKey] = secretKey;
            }

            if (lookupTable.Count == 0)
            {
                // Because SecretKeyMinLength is non-zero, this corner case may be impossible. No harm however.
                Logger.LogCritical(
                    2,
                    "Could not find a valid configuration for the '{ReceiverName}' WebHook receiver and instance " +
                    "'{Id}'. To receive '{ReceiverName}' WebHook requests for instance '{Id}', please add a " +
                    "non-empty configuration value.",
                    ReceiverName,
                    id,
                    ReceiverName,
                    id);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SignatureFilter_NoSecrets,
                    ReceiverName,
                    id);
                throw new InvalidOperationException(message);
            }

            // 4. Add the new lookup table to the dictionary. Parsing was successful.
            _lookupTables.TryAdd(id, lookupTable);

            return lookupTable;
        }
    }
}
