// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Routing;

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
        /// <seealso cref="Metadata.IWebHookSecurityMetadata.VerifyCodeParameter"/>
        public static int CodeParameterMinLength => 32;

        /// <summary>
        /// Gets the maximum length of the <see cref="CodeQueryParameterName"/> query parameter value and secret key
        /// for receivers using such a query parameter.
        /// </summary>
        /// <seealso cref="Metadata.IWebHookSecurityMetadata.VerifyCodeParameter"/>
        public static int CodeParameterMaxLength => 128;

        /// <summary>
        /// Gets the name of a query parameter containing the secret key used to verify some WebHook receivers'
        /// requests.
        /// </summary>
        /// <seealso cref="Metadata.IWebHookSecurityMetadata.VerifyCodeParameter"/>
        public static string CodeQueryParameterName => "code";

        /// <summary>
        /// <para>
        /// Gets the key used to retrieve a configuration entry indicating the
        /// <see cref="Filters.WebHookSecurityFilter.EnsureSecureConnection"/> should not ensure a secure (HTTPS)
        /// connection with the sender. Configuration value, if any, should parse as a <see cref="bool"/>. If that
        /// parsed configuration value is <see langword="true"/>, the HTTPS check is disabled. Otherwise i.e. if the
        /// configuration value does not exist, cannot be parsed, or parses as <see langword="false"/>,
        /// <see cref="Filters.WebHookSecurityFilter.EnsureSecureConnection"/> performs the check.
        /// </para>
        /// <para>
        /// Key corresponds to an <see cref="Extensions.Configuration.IConfiguration"/> entry in the default
        /// <see cref="IWebHookReceiverConfig"/> implementation
        /// </para>
        /// </summary>
        /// <remarks>
        /// Most, if not all, receiver configurations include
        /// <see cref="Filters.WebHookSecurityFilter.EnsureSecureConnection"/> calls. For example,
        /// <see cref="Filters.WebHookVerifyCodeFilter"/> calls the method if
        /// <see cref="Metadata.IWebHookSecurityMetadata.VerifyCodeParameter"/> is <see langword="true"/>.
        /// </remarks>
        public static string DisableHttpsCheckConfigurationKey => "MS_WebHookDisableHttpsCheck";

        /// <summary>
        /// Gets the prefix of keys used to retrieve a per-receiver or per-id configuration string. Configuration value
        /// often contains the receiver's secret key or keys. Key corresponds to an
        /// <see cref="Extensions.Configuration.IConfiguration"/> entry in the default
        /// <see cref="IWebHookReceiverConfig"/> implementation.
        /// </summary>
        public static string ReceiverConfigurationKeyPrefix => "MS_WebHookReceiverSecret_";

        /// <summary>
        /// Gets the name of the <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry containing the event name
        /// for the current request.
        /// </summary>
        /// <seealso cref="Metadata.IWebHookEventMetadata"/>
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

        // TODO: Remove or use. Was used as route name associated with the single action in the old world.
        /// <summary>
        /// Gets the name of the route for receiving generic WebHook requests.
        /// </summary>
        public static string ReceiverRouteName => "ReceiversAction";
    }
}
