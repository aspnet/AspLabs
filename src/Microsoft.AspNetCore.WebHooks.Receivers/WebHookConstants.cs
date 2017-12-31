// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Routing;
using Microsoft.Extensions.Configuration;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names and values used in WebHook receivers and handlers.
    /// </summary>
    public static class WebHookConstants
    {
        /// <summary>
        /// Gets the minimum length of the <see cref="CodeQueryParameterName"/> query parameter value and secret key
        /// for receivers using such a query parameter.
        /// </summary>
        /// <seealso cref="Metadata.IWebHookVerifyCodeMetadata"/>
        public static int CodeParameterMinLength => 32;

        /// <summary>
        /// Gets the maximum length of the <see cref="CodeQueryParameterName"/> query parameter value and secret key
        /// for receivers using such a query parameter.
        /// </summary>
        /// <seealso cref="Metadata.IWebHookVerifyCodeMetadata"/>
        public static int CodeParameterMaxLength => 128;

        /// <summary>
        /// Gets the name of a query parameter containing the secret key used to verify some WebHook receivers'
        /// requests.
        /// </summary>
        /// <seealso cref="Metadata.IWebHookVerifyCodeMetadata"/>
        public static string CodeQueryParameterName => "code";

        /// <summary>
        /// Gets the relative key name of the configuration value or (in a few cases)
        /// <see cref="IConfigurationSection"/> containing a receiver's default secret key(s). This value or section is
        /// an immediate child of the <see cref="SecretKeyConfigurationKeySectionKey"/> section for a receiver.
        /// </summary>
        public static string DefaultIdConfigurationKey => "default";

        /// <summary>
        /// Gets the key of a configuration value indicating the
        /// <see cref="Filters.WebHookSecurityFilter.EnsureSecureConnection"/> should not ensure a secure (HTTPS)
        /// connection with the sender. Configuration value, if any, should parse as a <see cref="bool"/>. If that
        /// parsed configuration value is <see langword="true"/>, the HTTPS check is disabled. Otherwise i.e. if the
        /// configuration value does not exist, cannot be parsed, or parses as <see langword="false"/>,
        /// <see cref="Filters.WebHookSecurityFilter.EnsureSecureConnection"/> performs the check.
        /// </summary>
        /// <remarks>
        /// All receiver configurations should include
        /// <see cref="Filters.WebHookSecurityFilter.EnsureSecureConnection"/> calls. For example,
        /// <see cref="Filters.WebHookVerifyCodeFilter"/> calls the method if the receiver implements
        /// <see cref="Metadata.IWebHookVerifyCodeMetadata"/> in its metadata service.
        /// </remarks>
        public static string DisableHttpsCheckConfigurationKey { get; } = ConfigurationPath.Combine(
            ReceiverConfigurationSectionKey,
            "DisableHttpsCheck");

        /// <summary>
        /// Gets the model name for the root of body-bound objects. Places errors for <c>data</c> parameters in
        /// consistent <see cref="Mvc.ModelBinding.ModelStateDictionary"/> entries, separate from entries for (for
        /// example) parameters bound to route values.
        /// </summary>
        public static string ModelStateBodyModelName => "$body";

        /// <summary>
        /// Gets the key of the <see cref="IConfigurationSection"/> containing all configuration values used in this
        /// package. Immediate children include global configuration values and receiver-specific
        /// <see cref="IConfigurationSection"/>s.
        /// </summary>
        public static string ReceiverConfigurationSectionKey => "WebHooks";

        /// <summary>
        /// Gets the relative key name of the <see cref="IConfigurationSection"/> containing secret keys. This section
        /// is an immediate child of the receiver-specific <see cref="IConfigurationSection"/>. For most receivers,
        /// immediate children of this section are id and secret key pairs.
        /// </summary>
        public static string SecretKeyConfigurationKeySectionKey => "SecretKey";

        /// <summary>
        /// Gets the name of the <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry containing the event name
        /// for the current request when there is a single event name.
        /// </summary>
        /// <seealso cref="Metadata.IWebHookEventMetadata"/>
        /// <seealso cref="Metadata.IWebHookEventFromBodyMetadata"/>
        public static string EventKeyName => "event";

        /// <summary>
        /// Gets the name of the <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry containing the receiver id
        /// for the current request.
        /// </summary>
        public static string IdKeyName => "id";

        /// <summary>
        /// Gets the name of the <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry containing the receiver
        /// name for the current request.
        /// </summary>
        /// <seealso cref="IWebHookReceiver"/>
        public static string ReceiverKeyName => "webHookReceiver";

        /// <summary>
        /// Gets the name of the <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry containing an indication
        /// the <see cref="WebHookReceiverExistsConstraint"/> successfully confirmed the request matched a configured
        /// receiver.
        /// </summary>
        public static string ReceiverExistsKeyName => nameof(WebHookReceiverExistsConstraint);
    }
}
