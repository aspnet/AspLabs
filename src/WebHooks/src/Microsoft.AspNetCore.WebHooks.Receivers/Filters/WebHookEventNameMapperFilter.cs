// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// <para>
    /// An <see cref="IAsyncResourceFilter"/> implementation which uses <see cref="IWebHookEventFromBodyMetadata"/> to
    /// determine the event names for a WebHook request. Reads the event names from the request body and makes them
    /// available for model binding and short-circuiting ping requests but not for action selection.
    /// </para>
    /// <para>
    /// This filter accepts all requests for receivers lacking <see cref="IWebHookEventFromBodyMetadata"/> or with
    /// <see cref="IWebHookEventFromBodyMetadata.AllowMissing"/> set to <see langword="true"/>. Otherwise, the filter
    /// short-circuits requests with no event names in the body.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This filter ignores errors other filters will handle but rejects requests that cause model binding failures.
    /// </remarks>
    public class WebHookEventNameMapperFilter : IAsyncResourceFilter, IOrderedFilter
    {
        private readonly IWebHookBodyTypeMetadataService _bodyTypeMetadata;
        private readonly IWebHookEventFromBodyMetadata _eventFromBodyMetadata;
        private readonly ILogger _logger;
        private readonly WebHookMetadataProvider _metadataProvider;
        private readonly IWebHookRequestReader _requestReader;

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNameMapperFilter"/> instance to map event names using the given
        /// <paramref name="eventFromBodyMetadata"/>.
        /// </summary>
        /// <param name="requestReader">The <see cref="IWebHookRequestReader"/>.</param>
        /// <param name="loggerFactory">The <see creFf="ILoggerFactory"/>.</param>
        /// <param name="bodyTypeMetadata">The receiver's <see cref="IWebHookBodyTypeMetadataService"/>.</param>
        /// <param name="eventFromBodyMetadata">The receiver's <see cref="IWebHookEventFromBodyMetadata"/>.</param>
        public WebHookEventNameMapperFilter(
            IWebHookRequestReader requestReader,
            ILoggerFactory loggerFactory,
            IWebHookBodyTypeMetadataService bodyTypeMetadata,
            IWebHookEventFromBodyMetadata eventFromBodyMetadata)
            : this(requestReader, loggerFactory)
        {
            _bodyTypeMetadata = bodyTypeMetadata ?? throw new ArgumentNullException(nameof(bodyTypeMetadata));
            _eventFromBodyMetadata = eventFromBodyMetadata ?? throw new ArgumentNullException(nameof(eventFromBodyMetadata));
        }

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNameMapperFilter"/> instance to map event names using the
        /// receiver's <see cref="IWebHookEventFromBodyMetadata"/>. That metadata is found in
        /// <paramref name="metadataProvider"/>.
        /// </summary>
        /// <param name="requestReader">The <see cref="IWebHookRequestReader"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadataProvider">
        /// The <see cref="WebHookMetadataProvider"/> service. Searched for applicable metadata per-request.
        /// </param>
        /// <remarks>This overload is intended for use with <see cref="GeneralWebHookAttribute"/>.</remarks>
        public WebHookEventNameMapperFilter(
            IWebHookRequestReader requestReader,
            ILoggerFactory loggerFactory,
            WebHookMetadataProvider metadataProvider)
            : this(requestReader, loggerFactory)
        {
            _metadataProvider = metadataProvider ?? throw new ArgumentNullException(nameof(metadataProvider));
        }

        private WebHookEventNameMapperFilter(IWebHookRequestReader requestReader, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<WebHookEventNameMapperFilter>();
            _requestReader = requestReader ?? throw new ArgumentNullException(nameof(requestReader));;
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookEventNameMapperFilter"/>
        /// instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm WebHooks configuration is set up correctly (in <see cref="WebHookReceiverExistsFilter"/>).
        /// </item>
        /// <item>
        /// Confirm signature or <c>code</c> query parameter (e.g. in <see cref="WebHookVerifyCodeFilter"/> or a
        /// <see cref="WebHookVerifySignatureFilter"/> subclass).
        /// </item>
        /// <item>
        /// Confirm required headers, <see cref="RouteValueDictionary"/> entries and query parameters are provided (in
        /// <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in
        /// <see cref="WebHookGetHeadRequestFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventNameMapperConstraint"/> for this receiver
        /// (in this filter).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetHeadRequestFilter"/> for this receiver
        /// (in <see cref="WebHookPingRequestFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookVerifyBodyTypeFilter.Order + 10;

        /// <inheritdoc />
        int IOrderedFilter.Order => Order;

        /// <inheritdoc />
        public virtual async Task OnResourceExecutionAsync(
            ResourceExecutingContext context,
            ResourceExecutionDelegate next)
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
            var bodyTypeMetadata = _bodyTypeMetadata;
            var eventFromBodyMetadata = _eventFromBodyMetadata;
            if (bodyTypeMetadata == null)
            {
                if (!routeData.TryGetWebHookReceiverName(out var receiverName))
                {
                    await next();
                    return;
                }

                bodyTypeMetadata = _metadataProvider.GetBodyTypeMetadata(receiverName);
                eventFromBodyMetadata = _metadataProvider.GetEventFromBodyMetadata(receiverName);
                if (eventFromBodyMetadata == null)
                {
                    await next();
                    return;
                }
            }

            // No need to double-check the request's Content-Type. WebHookVerifyBodyTypeFilter would have
            // short-circuited the request if unsupported.
            StringValues eventNames;
            switch (bodyTypeMetadata.BodyType)
            {
                case WebHookBodyType.Form:
                    var form = await _requestReader.ReadAsFormDataAsync(context);
                    if (form == null)
                    {
                        // ReadAsFormDataAsync returns null only when other filters will log and return errors
                        // about the same conditions. Let those filters run.
                        await next();
                        return;
                    }

                    eventNames = form[eventFromBodyMetadata.BodyPropertyPath];
                    break;

                case WebHookBodyType.Json:
                    var json = await _requestReader.ReadBodyAsync<JContainer>(context);
                    if (json == null)
                    {
                        var modelState = context.ModelState;
                        if (modelState.IsValid)
                        {
                            // ReadAsJContainerAsync returns null when model state is valid only when other filters
                            // will log and return errors about the same conditions. Let those filters run.
                            await next();
                        }
                        else
                        {
                            context.Result = new BadRequestObjectResult(modelState);
                        }

                        return;
                    }

                    eventNames = ObjectPathUtilities.GetStringValues(json, eventFromBodyMetadata.BodyPropertyPath);
                    break;

                case WebHookBodyType.Xml:
                    var xml = await _requestReader.ReadBodyAsync<XElement>(context);
                    if (xml == null)
                    {
                        var modelState = context.ModelState;
                        if (modelState.IsValid)
                        {
                            // ReadAsXmlAsync returns null when model state is valid only when other filters will log
                            // and return errors about the same conditions. Let those filters run.
                            await next();
                        }
                        else
                        {
                            context.Result = new BadRequestObjectResult(modelState);
                        }

                        return;
                    }

                    eventNames = ObjectPathUtilities.GetStringValues(xml, eventFromBodyMetadata.BodyPropertyPath);
                    break;

                default:
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.General_InvalidEnumValue,
                        typeof(WebHookBodyType),
                        bodyTypeMetadata.BodyType);
                    throw new InvalidOperationException(message);
            }

            if (StringValues.IsNullOrEmpty(eventNames) && !eventFromBodyMetadata.AllowMissing)
            {
                var receiverName = bodyTypeMetadata.ReceiverName;
                _logger.LogWarning(
                    0,
                    "A '{ReceiverName}' WebHook request must contain a match for '{BodyPropertyPath}' in the HTTP " +
                    "request entity body indicating the type or types of event.",
                    receiverName,
                    eventFromBodyMetadata.BodyPropertyPath);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.EventMapper_NoBodyProperty,
                    receiverName,
                    eventFromBodyMetadata.BodyPropertyPath);
                context.Result = new BadRequestObjectResult(message);

                return;
            }

            routeData.SetWebHookEventNames(eventNames);

            await next();
        }
    }
}
