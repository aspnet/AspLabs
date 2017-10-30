// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// An <see cref="IAsyncResourceFilter"/> that verifies the Stripe request body. Confirms the body deserializes as
    /// a <see cref="JObject"/> that can be converted to a <see cref="StripeEvent"/>. Confirms notification identifier
    /// and event name are present and, unless it's a test event or the <c>MS_WebHookStripeDirect</c> configuration
    /// value is <see langword="true"/>, that the notification identifier can be used to retrieve event details.
    /// Short-circuits test events unless the <c>MS_WebHookStripePassThroughTestEvents</c> configuration value is
    /// <see langword="true"/>.
    /// </summary>
    public class StripeVerifyNotificationIdFilter : WebHookVerifyBodyContentFilter, IAsyncResourceFilter
    {
        // Serialize ModelState errors, especially top-level model binding issues, similarly to
        // CreateErrorResult(..., message, ...).
        private static readonly string ModelStateRootKey = WebHookErrorKeys.MessageKey;

        private readonly IModelBinder _bodyModelBinder;
        private readonly HttpClient _httpClient;
        private readonly ModelMetadata _jObjectMetadata;

        /// <summary>
        /// Instantiates a new <see cref="StripeVerifyNotificationIdFilter"/> instance.
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
        /// /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="optionsAccessor">
        /// The <see cref="IOptions{MvcOptions}"/> accessor for <see cref="MvcOptions"/>.
        /// </param>
        /// <param name="readerFactory">The <see cref="IHttpRequestStreamReaderFactory"/>.</param>
        public StripeVerifyNotificationIdFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            IModelMetadataProvider metadataProvider,
            IOptions<MvcOptions> optionsAccessor,
            IHttpRequestStreamReaderFactory readerFactory)
            : this(
                  configuration,
                  hostingEnvironment,
                  loggerFactory,
                  metadataProvider,
                  optionsAccessor,
                  readerFactory,
                  httpClient: null)
        {
        }

        // Allow tests to override the HttpClient.
        internal StripeVerifyNotificationIdFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            IModelMetadataProvider metadataProvider,
            IOptions<MvcOptions> optionsAccessor,
            IHttpRequestStreamReaderFactory readerFactory,
            HttpClient httpClient)
            : base(configuration, hostingEnvironment, loggerFactory)
        {
            var options = optionsAccessor.Value;
            _bodyModelBinder = new BodyModelBinder(options.InputFormatters, readerFactory, loggerFactory, options);
            _httpClient = httpClient ?? new HttpClient();
            _jObjectMetadata = metadataProvider.GetMetadataForType(typeof(JObject));
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all
        /// <see cref="StripeVerifyNotificationIdFilter"/> instances. This filter should execute just after
        /// <see cref="WebHookVerifyCodeFilter"/>.
        /// </summary>
        public new static int Order => WebHookSecurityFilter.Order + 5;

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
                Logger.LogError(
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
                Logger.LogError(
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

            // 5. Handle test events or get confirmed data.
            // `WebHookVerifyCodeFilter` has already handled the direct WebHook verification.
            if (IsTestEvent(notificationId))
            {
                // Will short-circuit test events if PassThroughTestEventsConfigurationKey is not set later, in
                // StripeTestEventResponseFilter.
                if (Configuration.IsTrue(StripeConstants.PassThroughTestEventsConfigurationKey))
                {
                    Logger.LogInformation(2, "Received a Stripe Test Event.");
                }
            }
            else if (!Configuration.IsTrue(StripeConstants.DirectWebHookConfigurationKey))
            {
                // Callback to get the real data.
                data = await GetEventDataAsync(context.HttpContext.Request, routeData, notificationId);
                if (data == null)
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.VerifyNotification_BadId,
                        notificationId);
                    context.Result = WebHookResultUtilities.CreateErrorResult(message);

                    return;
                }
            }

            // 6. Success. Provide event name and notification id for model binding.
            routeData.Values[WebHookConstants.EventKeyName] = eventName;
            routeData.Values[StripeConstants.NotificationIdKeyName] = notificationId;

            await next();
        }

        /// <summary>
        /// Returns the event data for this <paramref name="notificationId"/> from the authenticated source so that we
        /// know that it is valid.
        /// </summary>
        /// <param name="request">The incoming <see cref="HttpRequest">.</see></param>
        /// <param name="routeData">The <see cref="RouteData"/> for the <paramref name="request"/>.</param>
        /// <param name="notificationId">The notification identifier from the <paramref name="request"/> body.</param>
        /// <returns>
        /// A <see cref="Task{JObject}"/> that on completion provides a <see cref="JObject"/> containing event data for
        /// this <paramref name="notificationId"/>.
        /// </returns>
        protected virtual async Task<JObject> GetEventDataAsync(
            HttpRequest request,
            RouteData routeData,
            string notificationId)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }
            if (notificationId == null)
            {
                throw new ArgumentNullException(nameof(notificationId));
            }

            // Create HTTP request for requesting authoritative event data from Stripe
            var secretKey = GetSecretKey(
                ReceiverName,
                routeData,
                StripeConstants.SecretKeyMinLength,
                StripeConstants.SecretKeyMaxLength);
            var address = string.Format(
                CultureInfo.InvariantCulture,
                StripeConstants.EventUriTemplate,
                notificationId);
            var outgoing = new HttpRequestMessage(HttpMethod.Get, address);
            var challenge = Encoding.UTF8.GetBytes(secretKey + ":");
            outgoing.Headers.Authorization = new AuthenticationHeaderValue(
                "Basic",
                EncodingUtilities.ToBase64(challenge, uriSafe: false));

            using (var response = await _httpClient.SendAsync(outgoing))
            {
                if (!response.IsSuccessStatusCode)
                {
                    Logger.LogError(
                        3,
                        "The notification identifier '{NotificationId}' could not be resolved for an actual event. " +
                        "Callback failed with status code {StatusCode}",
                        notificationId,
                        response.StatusCode);
                    return null;
                }

                var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JObject.LoadAsync(new JsonTextReader(new StreamReader(responseStream)));

                return result;
            }
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
                !request.IsJson())
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
            await _bodyModelBinder.BindModelAsync(bindingContext);
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

        internal static bool IsTestEvent(string notificationId)
        {
            return string.Equals(
                StripeConstants.TestNotificationId,
                notificationId,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
