// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IResourceFilter"/> to verify secret keys and short-circuit WebHook GET or HEAD requests.
    /// </summary>
    public class WebHookGetHeadRequestFilter : WebHookSecurityFilter, IResourceFilter
    {
        private readonly IReadOnlyList<IWebHookGetHeadRequestMetadata> _getHeadRequestMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookGetHeadRequestFilter"/> instance.
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
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        public WebHookGetHeadRequestFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            IEnumerable<IWebHookMetadata> metadata)
            : base(configuration, hostingEnvironment, loggerFactory)
        {
            _getHeadRequestMetadata = metadata.OfType<IWebHookGetHeadRequestMetadata>().ToArray();
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookGetHeadRequestFilter"/>
        /// instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm signature or <c>code</c> query parameter e.g. in <see cref="WebHookVerifyCodeFilter"/> or other
        /// <see cref="WebHookSecurityFilter"/> subclass.
        /// </item>
        /// <item>
        /// Confirm required headers, <see cref="RouteValueDictionary"/> entries and query parameters are provided (in
        /// <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>Short-circuit GET or HEAD requests, if receiver supports either (in this filter).</item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventMapperConstraint"/> for this receiver (in
        /// <see cref="WebHookEventMapperFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in this filter for this receiver (in
        /// <see cref="WebHookPingRequestFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public new static int Order => WebHookVerifyRequiredValueFilter.Order + 10;

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            var request = context.HttpContext.Request;
            if (routeData.TryGetWebHookReceiverName(out var receiverName) &&
                (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method)))
            {
                var getHeadRequestMetadata = _getHeadRequestMetadata
                    .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (getHeadRequestMetadata != null)
                {
                    // First verify that we have the secret key configuration value. This may be redundant if the
                    // receiver also implements IWebHookVerifyCodeMetadata in its metadata. However this verification
                    // is necessary for some receivers because signature verification (for example) is not possible
                    // without a body.
                    var secretKey = GetSecretKey(
                        receiverName,
                        routeData,
                        getHeadRequestMetadata.SecretKeyMinLength,
                        getHeadRequestMetadata.SecretKeyMaxLength);
                    if (secretKey == null)
                    {
                        // Have already logged about this case.
                        context.Result = new NotFoundResult();
                        return;
                    }

                    if (HttpMethods.IsHead(request.Method))
                    {
                        if (getHeadRequestMetadata.AllowHeadRequests)
                        {
                            // Success #1
                            Logger.LogInformation(
                                400,
                                "Received a HEAD request for the '{ReceiverName}' WebHook receiver -- ignoring.",
                                receiverName);
                            context.Result = new OkResult();
                        }

                        // Never respond to a HEAD request with a challenge response.
                        return;
                    }

                    if (getHeadRequestMetadata.ChallengeQueryParameterName == null)
                    {
                        // Success #2: Simple GET case. Have done all necessary verification.
                        Logger.LogInformation(
                            401,
                            "Received a GET request for the '{ReceiverName}' WebHook receiver -- ignoring.",
                            receiverName);
                        context.Result = new OkResult();
                        return;
                    }

                    // Success #3 unless required query parameter is missing.
                    context.Result = GetChallengeResponse(getHeadRequestMetadata, receiverName, request, routeData);
                }
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        private IActionResult GetChallengeResponse(
            IWebHookGetHeadRequestMetadata getHeadRequestMetadata,
            string receiverName,
            HttpRequest request,
            RouteData routeData)
        {
            // Get the 'challenge' parameter from the request URI.
            var challenge = request.Query[getHeadRequestMetadata.ChallengeQueryParameterName];
            if (StringValues.IsNullOrEmpty(challenge))
            {
                Logger.LogWarning(
                    402,
                    "A '{ReceiverName}' WebHook verification request must contain a '{ParameterName}' query " +
                    "parameter.",
                    receiverName,
                    getHeadRequestMetadata.ChallengeQueryParameterName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.GetRequest_NoQueryParameter,
                    receiverName,
                    getHeadRequestMetadata.ChallengeQueryParameterName);
                var noChallenge = new BadRequestObjectResult(message);

                return noChallenge;
            }

            Logger.LogInformation(
                403,
                "Received a GET request for the '{ReceiverName}' WebHook receiver -- returning challenge response.",
                receiverName);

            // Echo the challenge back to the caller.
            return new ContentResult
            {
                Content = challenge,
            };
        }
    }
}
