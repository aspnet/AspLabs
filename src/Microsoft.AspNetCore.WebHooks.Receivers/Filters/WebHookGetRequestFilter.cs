// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    /// An <see cref="IResourceFilter"/> to verify secret keys and short-circuit WebHook GET requests.
    /// </summary>
    public class WebHookGetRequestFilter : WebHookSecurityFilter, IResourceFilter
    {
        private readonly IReadOnlyList<IWebHookGetRequestMetadata> _getRequestMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookGetRequestFilter"/> instance.
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
        public WebHookGetRequestFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            IEnumerable<IWebHookMetadata> metadata)
            : base(configuration, hostingEnvironment, loggerFactory)
        {
            _getRequestMetadata = metadata.OfType<IWebHookGetRequestMetadata>().ToArray();
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookGetRequestFilter"/>
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
                throw new System.ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            if (routeData.TryGetWebHookReceiverName(out var receiverName) &&
                HttpMethods.IsGet(context.HttpContext.Request.Method))
            {
                var getRequestMetadata = _getRequestMetadata
                    .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (getRequestMetadata != null)
                {
                    // First verify that we have the secret key configuration value. This may be redundant if the
                    // receiver also implements IWebHookVerifyCodeMetadata in its metadata. However this verification
                    // is necessary for some receivers because signature verification (for example) is not possible
                    // without a body.
                    var secretKey = GetSecretKey(
                        receiverName,
                        routeData,
                        getRequestMetadata.SecretKeyMinLength,
                        getRequestMetadata.SecretKeyMaxLength);
                    if (secretKey == null)
                    {
                        context.Result = new NotFoundResult();
                        return;
                    }

                    if (getRequestMetadata.ChallengeQueryParameterName == null)
                    {
                        // Simple case. Have done all necessary verification.
                        context.Result = new OkResult();
                        return;
                    }

                    var request = context.HttpContext.Request;
                    context.Result = GetChallengeResponse(getRequestMetadata, receiverName, request, routeData);
                }
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        private IActionResult GetChallengeResponse(
            IWebHookGetRequestMetadata getRequestMetadata,
            string receiverName,
            HttpRequest request,
            RouteData routeData)
        {
            // Get the 'challenge' parameter from the request URI.
            var challenge = request.Query[getRequestMetadata.ChallengeQueryParameterName];
            if (StringValues.IsNullOrEmpty(challenge))
            {
                Logger.LogError(
                    400,
                    "A '{ReceiverName}' WebHook verification request must contain a '{ParameterName}' query " +
                    "parameter.",
                    receiverName,
                    getRequestMetadata.ChallengeQueryParameterName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.GetRequest_NoQueryParameter,
                    receiverName,
                    getRequestMetadata.ChallengeQueryParameterName);
                var noChallenge = new BadRequestObjectResult(message);

                return noChallenge;
            }

            // Echo the challenge back to the caller.
            return new ContentResult
            {
                Content = challenge,
            };
        }
    }
}
