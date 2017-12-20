// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.Filters
{
    /// <summary>
    /// Base class for <see cref="Mvc.Filters.IResourceFilter"/> or <see cref="Mvc.Filters.IAsyncResourceFilter"/>
    /// implementations that for example verify request signatures or <c>code</c> query parameters. Subclasses may
    /// also implement <see cref="IWebHookReceiver"/>. Subclasses should have an
    /// <see cref="Mvc.Filters.IOrderedFilter.Order"/> equal to <see cref="Order"/>.
    /// </summary>
    public abstract class WebHookSecurityFilter
    {
        /// <summary>
        /// Instantiates a new <see cref="WebHookSecurityFilter"/> instance.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> used to initialize <see cref="Configuration"/>.
        /// </param>
        /// <param name="hostingEnvironment">
        /// The <see cref="IHostingEnvironment" /> used to initialize <see cref="HostingEnvironment"/>.
        /// </param>
        /// <param name="loggerFactory">
        /// The <see cref="ILoggerFactory"/> used to initialize <see cref="Logger"/>.
        /// </param>
        protected WebHookSecurityFilter(
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (hostingEnvironment == null)
            {
                throw new ArgumentNullException(nameof(hostingEnvironment));
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        /// Gets the <see cref="Mvc.Filters.IOrderedFilter.Order"/> recommended for all
        /// <see cref="WebHookSecurityFilter"/> instances. The recommended filter sequence is
        /// <list type="number">
        /// <item>
        /// Confirm signature or <c>code</c> query parameter e.g. in <see cref="WebHookVerifyCodeFilter"/> or other
        /// <see cref="WebHookSecurityFilter"/> subclass.
        /// </item>
        /// <item>
        /// Confirm required headers, <see cref="RouteValueDictionary"/> entries and query parameters are provided (in
        /// <see cref="WebHookVerifyRequiredValueFilter"/>).
        /// </item>
        /// <item>
        /// Short-circuit GET or HEAD requests, if receiver supports either (in <see cref="WebHookGetRequestFilter"/>).
        /// </item>
        /// <item>Confirm it's a POST request (in <see cref="WebHookVerifyMethodFilter"/>).</item>
        /// <item>Confirm body type (in <see cref="WebHookVerifyBodyTypeFilter"/>).</item>
        /// <item>
        /// Short-circuit ping requests, if not done in <see cref="WebHookGetRequestFilter"/> for this receiver (in
        /// <see cref="WebHookPingRequestFilter"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => -500;

        /// <summary>
        /// Gets the <see cref="IConfiguration"/> for the application.
        /// </summary>
        protected IConfiguration Configuration;

        /// <summary>
        /// Gets the <see cref="IHostingEnvironment" />.
        /// </summary>
        protected IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Gets an <see cref="ILogger"/> for use in this class and any subclasses.
        /// </summary>
        /// <remarks>
        /// Methods in this class use <see cref="EventId"/>s that should be distinct from (higher than) those used in
        /// subclasses.
        /// </remarks>
        protected ILogger Logger { get; }

        /// <summary>
        /// Some WebHooks rely on HTTPS for sending WebHook requests in a secure manner. A
        /// <see cref="WebHookSecurityFilter"/> subclass can call this method to ensure that the incoming WebHook
        /// request is using HTTPS. If the request is not using HTTPS an error will be generated and the request will
        /// not be further processed.
        /// </summary>
        /// <remarks>
        /// This method allows HTTP requests while the application is in development or if the
        /// <see cref="WebHookConstants.DisableHttpsCheckConfigurationKey"/> is <see langword="true"/>.
        /// </remarks>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <param name="request">The current <see cref="HttpRequest"/>.</param>
        /// <returns>
        /// <see langword="null"/> in the success case. When a check fails, an <see cref="IActionResult"/> that when
        /// executed will produce a response containing details about the problem.
        /// </returns>
        protected virtual IActionResult EnsureSecureConnection(string receiverName, HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Check to see if we have been configured to ignore this check.
            var disableHttpsCheckString = Configuration[WebHookConstants.DisableHttpsCheckConfigurationKey];
            if (HostingEnvironment.IsDevelopment() ||
                (bool.TryParse(disableHttpsCheckString, out var disableHttpsCheck) && disableHttpsCheck))
            {
                return null;
            }

            // Require HTTPS.
            if (!request.IsHttps)
            {
                Logger.LogError(
                    500,
                    "The '{ReceiverName}' WebHook receiver requires {UpperSchemeName} in order to be secure. " +
                    "Please register a WebHook URI of type '{SchemeName}'.",
                    receiverName,
                    Uri.UriSchemeHttps.ToUpper(),
                    Uri.UriSchemeHttps);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Security_NoHttps,
                    receiverName,
                    Uri.UriSchemeHttps.ToUpper(),
                    Uri.UriSchemeHttps);
                var noHttps = new BadRequestObjectResult(message);

                return noHttps;
            }

            return null;
        }

        /// <summary>
        /// Gets the locally configured WebHook secret key used to validate any signature header provided in a WebHook
        /// request.
        /// </summary>
        /// <param name="sectionKey">
        /// The key (relative to <see cref="WebHookConstants.ReceiverConfigurationSectionKey"/>) of the
        /// <see cref="IConfigurationSection"/> containing the receiver-specific
        /// <see cref="WebHookConstants.SecretKeyConfigurationKeySectionKey"/> <see cref="IConfigurationSection"/>.
        /// Typically this is the name of the receiver e.g. <c>github</c>.
        /// </param>
        /// <param name="routeData">
        /// The <see cref="RouteData"/> for this request. A (potentially empty) ID value in this data allows a
        /// <see cref="WebHookSecurityFilter"/> subclass to support multiple senders with individual configurations.
        /// </param>
        /// <param name="minLength">The minimum length of the key value.</param>
        /// <param name="maxLength">The maximum length of the key value.</param>
        /// <returns>
        /// The configured WebHook secret key. <see langword="null"/> if the configuration value does not exist.
        /// </returns>
        protected virtual string GetSecretKey(
            string sectionKey,
            RouteData routeData,
            int minLength,
            int maxLength)
        {
            if (sectionKey == null)
            {
                throw new ArgumentNullException(nameof(sectionKey));
            }
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            // Look up configuration for this receiver and instance.
            var secrets = GetSecretKeys(sectionKey, routeData);
            if (!secrets.Exists())
            {
                return null;
            }

            var secret = secrets.Value;
            if (secret == null)
            {
                // Strange case: User incorrectly configured this id with sub-keys.
                return null;
            }

            if (secret.Length < minLength || secret.Length > maxLength)
            {
                // Secrete key found but it does not meet the length requirements.
                routeData.TryGetWebHookReceiverId(out var id);
                Logger.LogCritical(
                    501,
                    "Could not find a valid configuration for the '{ReceiverName}' WebHook receiver, instance " +
                    "'{Id}'. The value must be between {MinLength} and {MaxLength} characters long.",
                    sectionKey,
                    id,
                    minLength,
                    maxLength);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.Security_BadSecret,
                    sectionKey,
                    id,
                    minLength,
                    maxLength);
                throw new InvalidOperationException(message);
            }

            return secret;
        }

        /// <summary>
        /// Gets the locally configured WebHook secret keys used to validate any signature header provided in a WebHook
        /// request.
        /// </summary>
        /// <param name="sectionKey">
        /// The key (relative to <see cref="WebHookConstants.ReceiverConfigurationSectionKey"/>) of the
        /// <see cref="IConfigurationSection"/> containing the receiver-specific
        /// <see cref="WebHookConstants.SecretKeyConfigurationKeySectionKey"/> <see cref="IConfigurationSection"/>.
        /// Typically this is the name of the receiver e.g. <c>github</c>.
        /// </param>
        /// <param name="routeData">
        /// The <see cref="RouteData"/> for this request. A (potentially empty) ID value in this data allows a
        /// <see cref="WebHookSecurityFilter"/> subclass to support multiple senders with individual configurations.
        /// </param>
        /// <returns>
        /// The <see cref="IConfigurationSection"/> containing the configured WebHook secret keys.
        /// <see langword="null"/> if the <see cref="IConfigurationSection"/> does not exist.
        /// </returns>
        protected virtual IConfigurationSection GetSecretKeys(string sectionKey, RouteData routeData)
        {
            if (sectionKey == null)
            {
                throw new ArgumentNullException(nameof(sectionKey));
            }
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            routeData.TryGetWebHookReceiverId(out var id);

            // Look up configuration for this receiver and instance
            var secrets = GetSecretKeys(Configuration, sectionKey, id);
            if (!secrets.Exists())
            {
                if (!HasSecretKeys(Configuration, sectionKey))
                {
                    // No secret key configuration for this receiver at all.
                    Logger.LogCritical(
                        502,
                        "Could not find a valid configuration for the '{ReceiverName}' WebHook receiver.",
                        sectionKey);

                    var message = string.Format(CultureInfo.CurrentCulture, Resources.Security_NoSecrets, sectionKey);
                    throw new InvalidOperationException(message);
                }

                // ID was not configured or the key length is invalid. Caller should treat null return value with a
                // Not Found response.
                Logger.LogError(
                    503,
                    "Could not find a valid configuration for the '{ReceiverName}' WebHook receiver, instance '{Id}'.",
                    sectionKey,
                    id);
            }

            return secrets;
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

        private static IConfigurationSection GetSecretKeys(IConfiguration configuration, string sectionKey, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = WebHookConstants.DefaultIdConfigurationKey;
            }

            // Look up configuration value for these keys.
            var key = ConfigurationPath.Combine(
                WebHookConstants.ReceiverConfigurationSectionKey,
                sectionKey,
                WebHookConstants.SecretKeyConfigurationKeySectionKey,
                id);

            return configuration.GetSection(key);
        }

        private static bool HasSecretKeys(IConfiguration configuration, string sectionKey)
        {
            var key = ConfigurationPath.Combine(
                WebHookConstants.ReceiverConfigurationSectionKey,
                sectionKey,
                WebHookConstants.SecretKeyConfigurationKeySectionKey);

            return configuration.GetSection(key).Exists();
        }
    }
}
