// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IAsyncResourceFilter"/> that verifies the Stripe signature header. Confirms the header exists,
    /// parses the header, reads Body bytes, and compares the hashes.
    /// </summary>
    public class StripeVerifySignatureFilter : WebHookVerifySignatureFilter, IAsyncResourceFilter
    {
        private static readonly char[] CommaSeparator = new[] { ',' };
        private static readonly char[] EqualSeparator = new[] { '=' };

        /// <summary>
        /// Instantiates a new <see cref="StripeVerifySignatureFilter"/> instance.
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
        public StripeVerifySignatureFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory)
            : base(configuration, hostingEnvironment, loggerFactory)
        {
        }

        /// <inheritdoc />
        public override string ReceiverName => StripeConstants.ReceiverName;

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

            // 1. Confirm this filter applies.
            var routeData = context.RouteData;
            var request = context.HttpContext.Request;
            if (!routeData.TryGetWebHookReceiverName(out var receiverName) ||
                !IsApplicable(receiverName) ||
                !HttpMethods.IsPost(request.Method))
            {
                await next();
                return;
            }

            // 2. Confirm a secure connection.
            var errorResult = EnsureSecureConnection(ReceiverName, request);
            if (errorResult != null)
            {
                context.Result = errorResult;
                return;
            }

            // 3. Get the timestamp and expected signature(s) from the signature header.
            var header = GetRequestHeader(request, StripeConstants.SignatureHeaderName, out errorResult);
            if (errorResult != null)
            {
                context.Result = errorResult;
                return;
            }

            errorResult = ValidateHeader(header);
            if (errorResult != null)
            {
                context.Result = errorResult;
                return;
            }

            var timestamp = GetTimestamp(header);
            var signatures = GetSignatures(header);

            // 4. Get the configured secret key.
            var secretKey = GetSecretKey(
                ReceiverName,
                routeData,
                StripeConstants.SecretKeyMinLength,
                StripeConstants.SecretKeyMaxLength);
            if (secretKey == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            var secret = Encoding.UTF8.GetBytes(secretKey);
            var prefix = Encoding.UTF8.GetBytes(timestamp + ".");

            // 5. Get the actual hash of the request body.
            var actualHash = await ComputeRequestBodySha256HashAsync(request, secret, prefix);

            // 6. Verify that the actual hash matches one of the expected hashes.
            var match = false;
            foreach (var signature in signatures)
            {
                // While this looks repetitious compared to hex-encoding actualHash (once), a single v1 entry in the
                // header is the normal case. Expect multiple signatures only when rolling secret keys.
                var expectedHash = FromHex(signature.Value, StripeConstants.SignatureHeaderName);
                if (expectedHash == null)
                {
                    context.Result = CreateBadHexEncodingResult(StripeConstants.SignatureHeaderName);
                    return;
                }

                if (SecretEqual(expectedHash, actualHash))
                {
                    match = true;
                    break;
                }
            }

            if (!match)
            {
                // Log about the issue and short-circuit remainder of the pipeline.
                context.Result = CreateBadSignatureResult(StripeConstants.SignatureHeaderName);
                return;
            }

            // Success
            await next();
        }

        // Header contains a comma-separated collection of key / value pairs. Get the value for the "t" key.
        private StringSegment GetTimestamp(string header)
        {
            var pairs = new TrimmingTokenizer(header, CommaSeparator);
            foreach (var pair in pairs)
            {
                var keyValuePair = new TrimmingTokenizer(pair, EqualSeparator, maxCount: 2);
                var enumerator = keyValuePair.GetEnumerator();
                enumerator.MoveNext();
                if (StringSegment.Equals(enumerator.Current, StripeConstants.TimestampKey, StringComparison.Ordinal))
                {
                    enumerator.MoveNext();
                    return enumerator.Current;
                }
            }

            return StringSegment.Empty;
        }

        // Header contains a comma-separated collection of key / value pairs. Get all values for the "v1" key.
        private IEnumerable<StringSegment> GetSignatures(string header)
        {
            var pairs = new TrimmingTokenizer(header, CommaSeparator);
            foreach (var pair in pairs)
            {
                var keyValuePair = new TrimmingTokenizer(pair, EqualSeparator, maxCount: 2);
                var enumerator = keyValuePair.GetEnumerator();
                enumerator.MoveNext();
                if (StringSegment.Equals(enumerator.Current, StripeConstants.SignatureKey, StringComparison.Ordinal))
                {
                    enumerator.MoveNext();
                    yield return enumerator.Current;
                }
            }
        }

        private IActionResult ValidateHeader(string header)
        {
            var hasTimestamp = false;
            var hasSignature = false;
            var pairs = new TrimmingTokenizer(header, CommaSeparator);
            foreach (var pair in pairs)
            {
                var keyValuePair = new TrimmingTokenizer(pair, EqualSeparator, maxCount: 2);
                if (keyValuePair.Count != 2)
                {
                    // Header is not formatted correctly.
                    Logger.LogWarning(
                        0,
                        $"The '{StripeConstants.SignatureHeaderName}' header value is invalid. '{{InvalidPair}}' " +
                        "should be a 'key=value' pair.",
                        pair);

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.SignatureFilter_InvalidHeaderFormat,
                        StripeConstants.SignatureHeaderName);
                    return new BadRequestObjectResult(message);
                }

                var enumerator = keyValuePair.GetEnumerator();
                enumerator.MoveNext();

                var key = enumerator.Current;
                if (StringSegment.Equals(key, StripeConstants.SignatureKey, StringComparison.Ordinal))
                {
                    enumerator.MoveNext();
                    hasSignature = !StringSegment.IsNullOrEmpty(enumerator.Current);
                }
                else if (StringSegment.Equals(key, StripeConstants.TimestampKey, StringComparison.Ordinal))
                {
                    enumerator.MoveNext();
                    hasTimestamp = !StringSegment.IsNullOrEmpty(enumerator.Current);
                }
            }

            if (!hasSignature)
            {
                Logger.LogWarning(
                    1,
                    $"The '{StripeConstants.SignatureHeaderName}' header value is invalid. Does not contain a " +
                    $"timestamp ('{StripeConstants.SignatureKey}') value.");
            }

            if (!hasTimestamp)
            {
                Logger.LogWarning(
                    2,
                    $"The '{StripeConstants.SignatureHeaderName}' header value is invalid. Does not contain a " +
                    $"signature ('{StripeConstants.TimestampKey}') value.");
            }

            if (!hasSignature || !hasTimestamp)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SignatureFilter_HeaderMissingValue,
                    StripeConstants.SignatureHeaderName,
                    StripeConstants.TimestampKey,
                    StripeConstants.SignatureKey);
                return new BadRequestObjectResult(message);
            }

            // Success
            return null;
        }
    }
}
