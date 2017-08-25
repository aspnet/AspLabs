// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> that verifies the Dropbox signature header. Confirms the header exists, reads
    /// Body bytes, and compares the hashes.
    /// </summary>
    public class DropboxVerifySignatureFilter : WebHookVerifyBodyContentFilter, IAsyncResourceFilter
    {
        /// <summary>
        /// Instantiates a new <see cref="DropboxVerifySignatureFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to initialize <see cref="WebHookSecurityFilter.Logger"/>.
        /// </param>
        /// <param name="receiverConfig">
        /// The <see cref="IWebHookReceiverConfig"/> used to initialize
        /// <see cref="WebHookSecurityFilter.Configuration"/> and <see cref="WebHookSecurityFilter.ReceiverConfig"/>.
        /// </param>
        public DropboxVerifySignatureFilter(ILoggerFactory loggerFactory, IWebHookReceiverConfig receiverConfig)
            : base(loggerFactory, receiverConfig)
        {
        }

        /// <inheritdoc />
        public override string ReceiverName => DropboxConstants.ReceiverName;

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
                // 1. Get the expected hash from the signature header.
                var header = GetRequestHeader(request, DropboxConstants.SignatureHeaderName, out var errorResult);
                if (errorResult != null)
                {
                    context.Result = errorResult;
                    return;
                }

                var expectedHash = GetDecodedHash(header, DropboxConstants.SignatureHeaderName, out errorResult);
                if (errorResult != null)
                {
                    context.Result = errorResult;
                    return;
                }

                // 2. Get the configured secret key.
                var secretKey = await GetReceiverConfig(
                    request,
                    routeData,
                    ReceiverName,
                    DropboxConstants.SecretKeyMinLength,
                    DropboxConstants.SecretKeyMaxLength);
                if (secretKey == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var secret = Encoding.UTF8.GetBytes(secretKey);

                // 3. Get the actual hash of the request body.
                var actualHash = await GetRequestBodyHash_SHA256(request, secret);

                // 4. Verify that the actual hash matches the expected hash.
                if (!SecretEqual(expectedHash, actualHash))
                {
                    // Log about the issue and short-circuit remainder of the pipeline.
                    errorResult = CreateBadSignatureResult(receiverName, DropboxConstants.SignatureHeaderName);

                    context.Result = errorResult;
                    return;
                }
            }

            await next();
        }
    }
}
