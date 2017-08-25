// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;          // ??? Will we run FxCop on the AspNetCore projects?
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// Base class for <see cref="Mvc.Filters.IResourceFilter"/> or <see cref="Mvc.Filters.IAsyncResourceFilter"/>
    /// implementations that for example verify request signatures or <c>code</c> query parameters. Subclasses may
    /// also implement <see cref="IWebHookReceiver"/>. Subclasses should have an
    /// <see cref="Mvc.Filters.IOrderedFilter.Order"/> less than <see cref="Order"/>.
    /// </summary>
    public abstract class WebHookSecurityFilter
    {
        /// <summary>
        /// Instantiates a new <see cref="WebHookSecurityFilter"/> instance.
        /// </summary>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to initialize <see cref="Logger"/>.
        /// </param>
        /// <param name="receiverConfig">
        /// The <see cref="IWebHookReceiverConfig"/> used to initialize <see cref="Configuration"/> and
        /// <see cref="ReceiverConfig"/>.
        /// </param>
        protected WebHookSecurityFilter(ILoggerFactory loggerFactory, IWebHookReceiverConfig receiverConfig)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            if (receiverConfig == null)
            {
                throw new ArgumentNullException(nameof(receiverConfig));
            }

            Logger = loggerFactory.CreateLogger(GetType());
            ReceiverConfig = receiverConfig;
        }

        /// <summary>
        /// Gets the <see cref="Mvc.Filters.IOrderedFilter.Order"/> recommended for all
        /// <see cref="WebHookSecurityFilter"/> instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm signature or <c>code</c> query parameter (e.g. in <see cref="WebHookVerifyCodeFilter"/> or a
        /// <see cref="WebHookVerifyBodyContentFilter"/> subclass).
        /// </item>
        /// <item>
        /// Confirm required headers and query parameters are provided (in
        /// <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in
        /// <see cref="WebHookGetResponseFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetResponseFilter"/> for this receiver (in
        /// <see cref="WebHookPingResponseFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => -500;

        /// <summary>
        /// Gets the current <see cref="IConfiguration"/> for the application.
        /// </summary>
        protected IConfiguration Configuration => ReceiverConfig.Configuration;

        /// <summary>
        /// Gets an <see cref="ILogger"/> for use in this class and any subclasses.
        /// </summary>
        /// <remarks>
        /// Methods in this class use <see cref="EventId"/>s that should be distinct from (higher than) those used in
        /// subclasses.
        /// </remarks>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the <see cref="IWebHookReceiverConfig"/> for WebHook receivers in this application.
        /// </summary>
        protected IWebHookReceiverConfig ReceiverConfig { get; }

        // ??? Why is this called so rarely? See Dropbox, GitHub and Pusher filters and corresponding old receivers.
        /// <summary>
        /// Some WebHooks rely on HTTPS for sending WebHook requests in a secure manner. A
        /// <see cref="WebHookSecurityFilter"/> subclass can call this method to ensure that the incoming WebHook
        /// request is using HTTPS. If the request is not using HTTPS an error will be generated and the request will
        /// not be further processed.
        /// </summary>
        /// <remarks>This method does allow local HTTP requests using <c>localhost</c>.</remarks>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <returns>
        /// <see langword="null"/> in the success case. When a check fails, an <see cref="IActionResult"/> that when
        /// executed will produce a response containing details about the problem.
        /// </returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller.")]
        protected virtual IActionResult EnsureSecureConnection(string receiverName, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Check to see if we have been configured to ignore this check
            if (ReceiverConfig.IsTrue(WebHookConstants.DisableHttpsCheckConfigurationKey))
            {
                return null;
            }

            // Require HTTP unless request is local
            if (!request.IsLocal() && !request.IsHttps)
            {
                Logger.LogError(
                    500,
                    "The '{ReceiverName}' WebHook receiver requires HTTPS in order to be secure. " +
                    "Please register a WebHook URI of type '{SchemeName}'.",
                    receiverName,
                    Uri.UriSchemeHttps);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Security_NoHttps,
                    receiverName,
                    Uri.UriSchemeHttps);
                var noHttps = WebHookResultUtilities.CreateErrorResult(message);

                return noHttps;
            }

            return null;
        }

        /// <summary>
        /// Gets the locally configured WebHook secret key used to validate any signature header provided in a WebHook
        /// request.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <param name="routeData">
        /// The <see cref="RouteData"/> for this request. A (potentially empty) ID value in this data allows a
        /// <see cref="WebHookSecurityFilter"/> subclass to support multiple senders with individual configurations.
        /// </param>
        /// <param name="configurationName">
        /// The name of the configuration to obtain. Typically this the name of the receiver, e.g. <c>github</c>.
        /// </param>
        /// <param name="minLength">The minimum length of the key value.</param>
        /// <param name="maxLength">The maximum length of the key value.</param>
        /// <returns>A <see cref="Task"/> that on completion provides the configured WebHook secret key.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by caller")]
        protected async virtual Task<string> GetReceiverConfig(
            HttpRequest request,
            RouteData routeData,
            string configurationName,
            int minLength,
            int maxLength)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }
            if (configurationName == null)
            {
                throw new ArgumentNullException(nameof(configurationName));
            }

            routeData.TryGetReceiverId(out var id);

            // Look up configuration for this receiver and instance
            var secret = await ReceiverConfig.GetReceiverConfigAsync(configurationName, id, minLength, maxLength);
            if (secret == null)
            {
                if (string.IsNullOrEmpty(id))
                {
                    // Either no configuration for this receiver at all or the key length is invalid.
                    Logger.LogCritical(
                        501,
                        "Could not find a valid configuration for WebHook receiver '{ReceiverName}'. The setting " +
                        "must be set to a value between {MinLength} and {MaxLength} characters long.",
                        configurationName,
                        minLength,
                        maxLength);

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Security_BadSecret,
                        configurationName,
                        id,
                        minLength,
                        maxLength);
                    throw new InvalidOperationException(message);
                }
                else
                {
                    // ID was not configured. Caller should treat null return value with a Not Found response.
                    Logger.LogError(
                        502,
                        "Could not find a valid configuration for WebHook receiver '{ReceiverName}' and instance " +
                        "'{Id}'. The setting must be set to a value between {MinLength} and {MaxLength} characters " +
                        "long.",
                        configurationName,
                        id,
                        minLength,
                        maxLength);
                }
            }

            return secret;
        }

        /// <summary>
        /// Provides a time consistent comparison of two secrets in the form of two strings.
        /// </summary>
        /// <param name="inputA">The first secret to compare.</param>
        /// <param name="inputB">The second secret to compare.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the two secrets are equal; <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        protected internal static bool SecretEqual(string inputA, string inputB)
        {
            if (ReferenceEquals(inputA, inputB))
            {
                return true;
            }

            if (inputA == null || inputB == null || inputA.Length != inputB.Length)
            {
                return false;
            }

            var areSame = true;
            for (var i = 0; i < inputA.Length; i++)
            {
                areSame &= inputA[i] == inputB[i];
            }

            return areSame;
        }
    }
}
