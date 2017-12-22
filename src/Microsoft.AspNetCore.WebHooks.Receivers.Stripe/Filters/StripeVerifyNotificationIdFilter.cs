// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        private readonly IWebHookRequestReader _requestReader;

        /// <summary>
        /// Instantiates a new <see cref="StripeVerifyNotificationIdFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="requestReader">The <see cref="IWebHookRequestReader"/>.</param>
        public StripeVerifyNotificationIdFilter(ILoggerFactory loggerFactory, IWebHookRequestReader requestReader)
        {
            _logger = loggerFactory.CreateLogger<StripeVerifyNotificationIdFilter>();
            _requestReader = requestReader;
        }

        /// <summary>
        /// Gets the <see cref="IOrderedFilter.Order"/> recommended for all
        /// <see cref="StripeVerifyNotificationIdFilter"/> instances. This filter should execute in the same slot as
        /// <see cref="WebHookVerifyRequiredValueFilter"/>, after <see cref="StripeVerifySignatureFilter"/> and before
        /// <see cref="StripeTestEventRequestFilter"/>. <see cref="WebHookVerifyRequiredValueFilter"/> does not apply
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
            var data = await _requestReader.ReadBodyAsync<JObject>(context);
            if (data == null)
            {
                var modelState = context.ModelState;
                if (modelState.IsValid)
                {
                    // ReadAsJObjectAsync returns null when model state is valid only when other filters will log and
                    // return errors about the same conditions. Let those filters run.
                    await next();
                }
                else
                {
                    context.Result = new BadRequestObjectResult(modelState);
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
                context.Result = new BadRequestObjectResult(message);

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
                context.Result = new BadRequestObjectResult(message);

                return;
            }

            // 5. Success. Provide event name and notification id for model binding.
            routeData.Values[WebHookConstants.EventKeyName] = eventName;
            routeData.Values[StripeConstants.NotificationIdKeyName] = notificationId;

            await next();
        }
    }
}
