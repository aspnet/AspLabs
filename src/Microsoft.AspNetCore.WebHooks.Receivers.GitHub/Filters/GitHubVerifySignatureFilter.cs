// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    /// An <see cref="IAsyncResourceFilter"/> that verifies the GitHub signature header. Confirms the header exists,
    /// reads Body bytes, and compares the hashes.
    /// </summary>
    public class GitHubVerifySignatureFilter : WebHookVerifySignatureFilter, IAsyncResourceFilter
    {
        // Character that appears between the key and value in the signature header.
        private static readonly char[] PairSeparators = new[] { '=' };

        /// <summary>
        /// Instantiates a new <see cref="GitHubVerifySignatureFilter"/> instance.
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
        public GitHubVerifySignatureFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory)
            : base(configuration, hostingEnvironment, loggerFactory)
        {
        }

        /// <inheritdoc />
        public override string ReceiverName => GitHubConstants.ReceiverName;

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
            if (routeData.TryGetWebHookReceiverName(out var receiverName) &&
                IsApplicable(receiverName) &&
                HttpMethods.IsPost(request.Method))
            {
                // 1. Confirm a secure connection.
                var errorResult = EnsureSecureConnection(ReceiverName, context.HttpContext.Request);
                if (errorResult != null)
                {
                    context.Result = errorResult;
                    return;
                }

                // 2. Get the expected hash from the signature header.
                var header = GetRequestHeader(request, GitHubConstants.SignatureHeaderName, out errorResult);
                if (errorResult != null)
                {
                    context.Result = errorResult;
                    return;
                }

                var values = new TrimmingTokenizer(header, PairSeparators);
                var enumerator = values.GetEnumerator();
                enumerator.MoveNext();
                var headerKey = enumerator.Current;
                if (values.Count != 2 ||
                    !StringSegment.Equals(
                        headerKey,
                        GitHubConstants.SignatureHeaderKey,
                        StringComparison.OrdinalIgnoreCase))
                {
                    Logger.LogError(
                        1,
                        "Invalid '{HeaderName}' header value. Expecting a value of '{Key}={Value}'.",
                        GitHubConstants.SignatureHeaderName,
                        GitHubConstants.SignatureHeaderKey,
                        "<value>");

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.SignatureFilter_BadHeaderValue,
                        GitHubConstants.SignatureHeaderName,
                        GitHubConstants.SignatureHeaderKey,
                        "<value>");
                    errorResult = new BadRequestObjectResult(message);

                    context.Result = errorResult;
                    return;
                }

                enumerator.MoveNext();
                var headerValue = enumerator.Current.Value;
                var expectedHash = GetDecodedHash(headerValue, GitHubConstants.SignatureHeaderName, out errorResult);
                if (errorResult != null)
                {
                    context.Result = errorResult;
                    return;
                }

                // 3. Get the configured secret key.
                var secretKey = GetSecretKey(
                    ReceiverName,
                    routeData,
                    GitHubConstants.SecretKeyMinLength,
                    GitHubConstants.SecretKeyMaxLength);
                if (secretKey == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var secret = Encoding.UTF8.GetBytes(secretKey);

                // 4. Get the actual hash of the request body.
                var actualHash = await GetRequestBodyHash_SHA1(request, secret);

                // 5. Verify that the actual hash matches the expected hash.
                if (!SecretEqual(expectedHash, actualHash))
                {
                    // Log about the issue and short-circuit remainder of the pipeline.
                    errorResult = CreateBadSignatureResult(receiverName, GitHubConstants.SignatureHeaderName);

                    context.Result = errorResult;
                    return;
                }
            }

            await next();
        }
    }
}
