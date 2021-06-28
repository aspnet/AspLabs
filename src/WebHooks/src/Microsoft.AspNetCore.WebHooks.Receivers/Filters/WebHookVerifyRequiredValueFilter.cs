// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// <para>
    /// An <see cref="IResourceFilter"/> to verify required HTTP headers, <see cref="RouteValueDictionary"/> entries
    /// and query parameters are present in a WebHook request. Uses <see cref="IWebHookBindingMetadata"/> services to
    /// determine the requirements for the requested WebHook receiver.
    /// </para>
    /// <para>
    /// Short-circuits the request if required values are missing. The response in that case will have a 400
    /// "Bad Request" status code.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The <see cref="Routing.WebHookEventNameMapperConstraint"/> and <see cref="WebHookVerifyCodeFilter"/> also
    /// verify required HTTP headers, <see cref="RouteValueDictionary"/> entries and query parameters. But, those
    /// constraints and filters do not use <see cref="IWebHookBindingMetadata"/> information.
    /// </remarks>
    public class WebHookVerifyRequiredValueFilter : IResourceFilter, IOrderedFilter
    {
        private readonly IWebHookBindingMetadata _bindingMetadata;
        private readonly ILogger _logger;
        private readonly WebHookMetadataProvider _metadataProvider;

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyRequiredValueFilter"/> instance to verify the given
        /// <paramref name="bindingMetadata"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="bindingMetadata">The receiver's <see cref="IWebHookBindingMetadata"/>.</param>
        public WebHookVerifyRequiredValueFilter(
            ILoggerFactory loggerFactory,
            IWebHookBindingMetadata bindingMetadata)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            _bindingMetadata = bindingMetadata ?? throw new ArgumentNullException(nameof(bindingMetadata));
            _logger = loggerFactory.CreateLogger<WebHookVerifyRequiredValueFilter>();
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookVerifyRequiredValueFilter"/> instance to verify the receiver's
        /// <see cref="IWebHookBindingMetadata"/>. That metadata is found in <paramref name="metadataProvider"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadataProvider">
        /// The <see cref="WebHookMetadataProvider"/> service. Searched for applicable metadata per-request.
        /// </param>
        /// <remarks>This overload is intended for use with <see cref="GeneralWebHookAttribute"/>.</remarks>
        public WebHookVerifyRequiredValueFilter(
            ILoggerFactory loggerFactory,
            WebHookMetadataProvider metadataProvider)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<WebHookVerifyRequiredValueFilter>();
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all
        /// <see cref="WebHookVerifyRequiredValueFilter"/> instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm WebHooks configuration is set up correctly (in <see cref="WebHookReceiverExistsFilter"/>).
        /// </item>
        /// <item>
        /// Confirm signature or <c>code</c> query parameter e.g. in <see cref="WebHookVerifyCodeFilter"/> or other
        /// <see cref="WebHookSecurityFilter"/> subclass.
        /// </item>
        /// <item>
        /// Confirm required headers, <see cref="RouteValueDictionary"/> entries and query parameters are provided (in
        /// this filter).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in
        /// <see cref="WebHookGetHeadRequestFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventNameMapperConstraint"/> for this receiver
        /// (in <see cref="WebHookEventNameMapperFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetHeadRequestFilter"/> for this receiver (in
        /// <see cref="WebHookPingRequestFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookSecurityFilter.Order + 10;

        /// <inheritdoc />
        int IOrderedFilter.Order => Order;

        /// <inheritdoc />
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var routeData = context.RouteData;
            var bindingMetadata = _bindingMetadata;
            if (bindingMetadata == null)
            {
                if (!routeData.TryGetWebHookReceiverName(out var requestReceiverName))
                {
                    return;
                }

                bindingMetadata = _metadataProvider.GetBindingMetadata(requestReceiverName);
                if (bindingMetadata == null)
                {
                    return;
                }
            }

            var receiverName = bindingMetadata.ReceiverName;
            var request = context.HttpContext.Request;
            for (var i = 0; i < bindingMetadata.Parameters.Count; i++)
            {
                var parameter = bindingMetadata.Parameters[i];
                if (parameter.IsRequired)
                {
                    bool found;
                    string message;
                    var sourceName = parameter.SourceName;
                    switch (parameter.ParameterType)
                    {
                        case WebHookParameterType.Header:
                            found = VerifyHeader(request.Headers, sourceName, receiverName, out message);
                            break;

                        case WebHookParameterType.RouteValue:
                            found = VerifyRouteData(routeData, sourceName, receiverName, out message);
                            break;

                        case WebHookParameterType.QueryParameter:
                            found = VerifyQueryParameter(request.Query, sourceName, receiverName, out message);
                            break;

                        default:
                            message = string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.General_InvalidEnumValue,
                                typeof(WebHookParameterType),
                                parameter.ParameterType);
                            throw new InvalidOperationException(message);
                    }

                    if (!found)
                    {
                        // Do not return after first error. Instead log about all issues.
                        context.Result = new BadRequestObjectResult(message);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No-op
        }

        private bool VerifyRouteData(RouteData routeData, string keyName, string receiverName, out string message)
        {
            if (routeData.Values.TryGetValue(keyName, out var value) && !string.IsNullOrEmpty(value as string))
            {
                message = null;
                return true;
            }

            _logger.LogWarning(
                0,
                "A '{ReceiverName}' WebHook request must contain a '{KeyName}' value in the route data.",
                receiverName,
                keyName);
            message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.VerifyRequiredValue_NoRouteValue,
                receiverName,
                keyName);

            return false;
        }

        private bool VerifyHeader(
            IHeaderDictionary headers,
            string headerName,
            string receiverName,
            out string message)
        {
            if (headers.TryGetValue(headerName, out var values) && !StringValues.IsNullOrEmpty(values))
            {
                message = null;
                return true;
            }

            _logger.LogWarning(
                1,
                "A '{ReceiverName}' WebHook request must contain a '{HeaderName}' HTTP header.",
                receiverName,
                headerName);
            message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.VerifyRequiredValue_NoHeader,
                receiverName,
                headerName);

            return false;
        }

        private bool VerifyQueryParameter(
            IQueryCollection query,
            string parameterName,
            string receiverName,
            out string message)
        {
            if (query.TryGetValue(parameterName, out var values) && !StringValues.IsNullOrEmpty(values))
            {
                message = null;
                return true;
            }

            _logger.LogWarning(
                2,
                "A '{ReceiverName}' WebHook request must contain a '{QueryParameterName}' query parameter.",
                receiverName,
                parameterName);
            message = string.Format(
                CultureInfo.CurrentCulture,
                Resources.General_NoQueryParameter,
                receiverName,
                parameterName);

            return false;
        }
    }
}
