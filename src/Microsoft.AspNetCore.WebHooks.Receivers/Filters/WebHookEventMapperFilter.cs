// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class WebHookEventMapperFilter : IAsyncResourceFilter
    {
        private readonly IReadOnlyList<IWebHookBodyTypeMetadataService> _bodyTypeMetadata;
        private readonly IReadOnlyList<IWebHookEventFromBodyMetadata> _eventFromBodyMetadata;
        private readonly ILogger _logger;
        private readonly IWebHookRequestReader _requestReader;

        /// <summary>
        /// Instantiates a new <see cref="WebHookEventMapperFilter"/> instance.
        /// </summary>
        /// <param name="bodyTypeMetadata">
        /// The collection of <see cref="IWebHookBodyTypeMetadataService"/> services.
        /// </param>
        /// <param name="eventFromBodyMetadata">
        /// The collection of <see cref="IWebHookEventFromBodyMetadata"/> services.
        /// </param>
        /// <param name="requestReader">The <see cref="IWebHookRequestReader"/>.</param>
        /// <param name="loggerFactory">The <see creFf="ILoggerFactory"/>.</param>
        public WebHookEventMapperFilter(
            IEnumerable<IWebHookBodyTypeMetadataService> bodyTypeMetadata,
            IEnumerable<IWebHookEventFromBodyMetadata> eventFromBodyMetadata,
            IWebHookRequestReader requestReader,
            ILoggerFactory loggerFactory)
        {
            _eventFromBodyMetadata = eventFromBodyMetadata.ToArray();

            // No need to track metadata unless it's applicable in this filter.
            _bodyTypeMetadata = bodyTypeMetadata
                .Where(bodyMetadata => _eventFromBodyMetadata.Any(
                    eventMetadata => eventMetadata.IsApplicable(bodyMetadata.ReceiverName)))
                .ToArray();

            _logger = loggerFactory.CreateLogger<WebHookEventMapperFilter>();
            _requestReader = requestReader;
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all <see cref="WebHookEventMapperFilter"/>
        /// instances. The recommended filter sequence is
        /// <list type="number">
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
        /// Map event name(s), if not done in <see cref="Routing.WebHookEventMapperConstraint"/> for this receiver (in
        /// this filter).
        /// </item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetHeadRequestFilter"/> for this receiver (in
        /// <see cref="WebHookPingRequestFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookVerifyBodyTypeFilter.Order + 10;

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
            if (!routeData.TryGetWebHookReceiverName(out var receiverName))
            {
                await next();
                return;
            }

            var eventFromBodyMetadata = _eventFromBodyMetadata
                .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
            if (eventFromBodyMetadata == null)
            {
                await next();
                return;
            }

            // Determine the applicable WebhookBodyType i.e. how to read the request body.
            // WebHookReceiverExistsConstraint confirms the IWebHookBodyTypeMetadataService implementation exists.
            var bodyTypeMetadata = _bodyTypeMetadata.First(metadata => metadata.IsApplicable(receiverName));

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
