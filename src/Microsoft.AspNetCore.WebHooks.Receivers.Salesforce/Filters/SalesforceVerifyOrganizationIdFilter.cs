// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
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
    // TODO: Create similar IActionFilter for use in Azure Alert, Dynamics CRM, Kudu, MailChimp, and Pusher receivers.
    // TODO:  Filter would require event name in request body and map that to route data. No action selection support.
    /// <summary>
    /// An <see cref="IAsyncResourceFilter"/> that verifies the Salesforce SOAP request body. Confirms the body
    /// deserializes as <see cref="XElement"/> that can be converted to <see cref="SalesforceNotifications"/>. Then
    /// confirms the organization id and event name is present and that the organization id matches the configured
    /// secret key.
    /// </summary>
    public class SalesforceVerifyOrganizationIdFilter : WebHookSecurityFilter, IAsyncResourceFilter, IWebHookReceiver
    {
        private readonly ISalesforceResultCreator _resultCreator;
        private readonly IWebHookRequestReader _requestReader;

        /// <summary>
        /// Instantiates a new <see cref="SalesforceVerifyOrganizationIdFilter"/> instance.
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
        /// <param name="resultCreator">The <see cref="ISalesforceResultCreator"/>.</param>
        /// <param name="requestReader">The <see cref="IWebHookRequestReader"/>.</param>
        public SalesforceVerifyOrganizationIdFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            ISalesforceResultCreator resultCreator,
            IWebHookRequestReader requestReader)
            : base(configuration, hostingEnvironment, loggerFactory)
        {
            _resultCreator = resultCreator;
            _requestReader = requestReader;
        }

        /// <inheritdoc />
        public string ReceiverName => SalesforceConstants.ReceiverName;

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

            // 2. Confirm we were reached using HTTPS.
            var request = context.HttpContext.Request;
            var errorResult = EnsureSecureConnection(receiverName, request);
            if (errorResult != null)
            {
                context.Result = errorResult;
                return;
            }

            // 3. Get XElement from the request body.
            var data = await _requestReader.ReadBodyAsync<XElement>(context);
            if (data == null)
            {
                var modelState = context.ModelState;
                if (modelState.IsValid)
                {
                    // ReadAsXmlAsync returns null when model state is valid only when other filters will log and
                    // return errors about the same conditions. Let those filters run.
                    await next();
                }
                else
                {
                    context.Result = new BadRequestObjectResult(modelState);
                }

                return;
            }

            // 4. Ensure the organization ID exists and matches the expected value.
            var organizationIds = ObjectPathUtilities.GetStringValues(data, SalesforceConstants.OrganizationIdPath);
            if (StringValues.IsNullOrEmpty(organizationIds))
            {
                Logger.LogError(
                    0,
                    "The HTTP request body did not contain a required '{XPath}' element.",
                    SalesforceConstants.OrganizationIdPath);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifyOrganization_MissingValue,
                    SalesforceConstants.OrganizationIdPath);
                context.Result = await _resultCreator.GetFailedResultAsync(message);

                return;
            }

            var secret = GetSecretKey(
                ReceiverName,
                routeData,
                SalesforceConstants.SecretKeyMinLength,
                SalesforceConstants.SecretKeyMaxLength);

            var organizationId = GetShortOrganizationId(organizationIds[0]);
            var secretKey = GetShortOrganizationId(secret);
            if (!SecretEqual(organizationId, secretKey))
            {
                Logger.LogError(
                    1,
                    "The '{XPath}' value provided in the HTTP request body did not match the expected value.",
                    SalesforceConstants.OrganizationIdPath);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifyOrganization_BadValue,
                    SalesforceConstants.OrganizationIdPath);
                context.Result = await _resultCreator.GetFailedResultAsync(message);

                return;
            }

            // 5. Get the event name.
            var eventNames = ObjectPathUtilities.GetStringValues(data, SalesforceConstants.EventNamePath);
            if (StringValues.IsNullOrEmpty(eventNames))
            {
                Logger.LogError(
                    2,
                    "The HTTP request body did not contain a required '{XPath}' element.",
                    SalesforceConstants.EventNamePath);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.VerifyOrganization_MissingValue,
                    SalesforceConstants.EventNamePath);
                context.Result = await _resultCreator.GetFailedResultAsync(message);

                return;
            }

            // 6. Success. Provide event name for model binding.
            routeData.SetWebHookEventNames(eventNames);

            await next();
        }

        /// <summary>
        /// Gets the shortened version of <paramref name="fullOrganizationId"/>.
        /// </summary>
        /// <param name="fullOrganizationId">The full organization name.</param>
        /// <returns>The shortened version of <paramref name="fullOrganizationId"/>.</returns>
        protected static string GetShortOrganizationId(string fullOrganizationId)
        {
            if (fullOrganizationId?.Length == 18)
            {
                return fullOrganizationId.Substring(0, 15);
            }

            return fullOrganizationId;
        }
    }
}
