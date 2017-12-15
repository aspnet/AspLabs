// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IAsyncResourceFilter"/> that verifies the Stripe request body. Confirms the body deserializes as
    /// a <see cref="JObject"/>. Confirms notification identifier and event name are present. Adds both strings to
    /// route values.
    /// </summary>
    public class StripeVerifyNotificationIdFilter : IAsyncResourceFilter, IWebHookReceiver
    {
        // Serialize ModelState errors, especially top-level model binding issues, similarly to
        // CreateErrorResult(..., message, ...).
        private static readonly string ModelStateRootKey = WebHookErrorKeys.MessageKey;

        private readonly IModelBinder _bodyModelBinder;
        private readonly ILogger _logger;
        private readonly ModelMetadata _jObjectMetadata;

        /// <summary>
        /// Instantiates a new <see cref="StripeVerifyNotificationIdFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="optionsAccessor">
        /// The <see cref="IOptions{MvcOptions}"/> accessor for <see cref="MvcOptions"/>.
        /// </param>
        /// <param name="readerFactory">The <see cref="IHttpRequestStreamReaderFactory"/>.</param>
        public StripeVerifyNotificationIdFilter(
            ILoggerFactory loggerFactory,
            IModelMetadataProvider metadataProvider,
            IOptions<MvcOptions> optionsAccessor,
            IHttpRequestStreamReaderFactory readerFactory)
        {
            var options = optionsAccessor.Value;
            _bodyModelBinder = new BodyModelBinder(options.InputFormatters, readerFactory, loggerFactory, options);
            _logger = loggerFactory.CreateLogger<StripeVerifyNotificationIdFilter>();
            _jObjectMetadata = metadataProvider.GetMetadataForType(typeof(JObject));
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all
        /// <see cref="StripeVerifyNotificationIdFilter"/> instances. This filter should execute in the same slot as
        /// <see cref="WebHookVerifyRequiredValueFilter"/>, after <see cref="StripeVerifySignatureFilter"/> and before
        /// <see cref="StripeTestEventResponseFilter"/>. <see cref="WebHookVerifyRequiredValueFilter"/> does not apply
        /// for this receiver; required parameters are enforced in this filter.
        /// </summary>
        public static int Order => WebHookVerifyRequiredValueFilter.Order;

        /// <inheritdoc />
        public string ReceiverName => StripeConstants.ReceiverName;

        /// <inheritdoc />
        public bool IsApplicable(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            return string.Equals(ReceiverName, receiverName, StringComparison.OrdinalIgnoreCase);
        }

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
            if (!routeData.TryGetWebHookReceiverName(out var receiverName) || !IsApplicable(receiverName))
            {
                await next();
                return;
            }

            // 2. Get JObject from the request body.
            var data = await ReadAsJsonAsync(context);
            if (data == null)
            {
                var modelState = context.ModelState;
                if (modelState.IsValid)
                {
                    // ReadAsJsonAsync returns null when model state is valid only when other filters will log and
                    // return errors about the same conditions. Let those filters run.
                    await next();
                }
                else
                {
                    context.Result = WebHookResultUtilities.CreateErrorResult(modelState);
                }

                return;
            }

            // 3. Ensure the notification identifier exists.
            var notificationId = data.Value<string>(StripeConstants.NotificationIdPropertyName);
            if (string.IsNullOrEmpty(notificationId))
            {
                _logger.LogError(
                    0,
                    "The HTTP request body did not contain a required '{PropertyName}' property.",
                    StripeConstants.NotificationIdPropertyName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifyNotification_MissingValue,
                    StripeConstants.NotificationIdPropertyName);
                context.Result = WebHookResultUtilities.CreateErrorResult(message);

                return;
            }

            // 4. Ensure the event name exists.
            var eventName = data.Value<string>(StripeConstants.EventPropertyName);
            if (string.IsNullOrEmpty(eventName))
            {
                _logger.LogError(
                    1,
                    "The HTTP request body did not contain a required '{PropertyName}' property.",
                    StripeConstants.EventPropertyName);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifyNotification_MissingValue,
                    StripeConstants.EventPropertyName);
                context.Result = WebHookResultUtilities.CreateErrorResult(message);

                return;
            }

            // 5. Success. Provide event name and notification id for model binding.
            routeData.Values[WebHookConstants.EventKeyName] = eventName;
            routeData.Values[StripeConstants.NotificationIdKeyName] = notificationId;

            await next();
        }

        /// <summary>
        /// Reads the JSON HTTP request entity body.
        /// </summary>
        /// <param name="context">The <see cref="ResourceExecutingContext"/>.</param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <see cref="JObject"/> containing the HTTP request entity
        /// body.
        /// </returns>
        protected virtual async Task<JObject> ReadAsJsonAsync(ResourceExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var request = context.HttpContext.Request;
            if (request.Body == null ||
                !request.ContentLength.HasValue ||
                request.ContentLength.Value == 0L ||
                !HttpMethods.IsPost(request.Method) ||
                !WebHookHttpRequestUtilities.IsJson(request))
            {
                // Other filters will log and return errors about these conditions.
                return null;
            }

            var modelState = context.ModelState;
            var actionContext = new ActionContext(
                context.HttpContext,
                context.RouteData,
                context.ActionDescriptor,
                modelState);

            var valueProviderFactories = context.ValueProviderFactories;
            var valueProvider = await CompositeValueProvider.CreateAsync(actionContext, valueProviderFactories);
            var bindingContext = DefaultModelBindingContext.CreateBindingContext(
                actionContext,
                valueProvider,
                _jObjectMetadata,
                bindingInfo: null,
                modelName: ModelStateRootKey);

            // Read request body.
            try
            {
                await _bodyModelBinder.BindModelAsync(bindingContext);
            }
            finally
            {
                request.Body.Seek(0L, SeekOrigin.Begin);
            }

            if (!bindingContext.ModelState.IsValid)
            {
                return null;
            }

            if (!bindingContext.Result.IsModelSet)
            {
                throw new InvalidOperationException(Resources.VerifyNotification_ModelBindingFailed);
            }

            // Success
            return (JObject)bindingContext.Result.Model;
        }
    }
}
