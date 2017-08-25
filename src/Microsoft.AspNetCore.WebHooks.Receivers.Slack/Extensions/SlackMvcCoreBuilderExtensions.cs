// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Slack WebHooks in an <see cref="IMvcCoreBuilder" />.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SlackMvcCoreBuilderExtensions
    {
        private static readonly Action<WebHookOptions> OptionsSetupAction = SetupWebHookOptions;

        /// <summary>
        /// Add Slack WebHook configuration and services to the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddSlackWebHooks(this IMvcCoreBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var services = builder.Services;
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IWebHookMetadata, SlackMetadata>());

            // While requests contain HTTP form data, responses are JSON.
            return builder
                .AddJsonFormatters()
                .AddWebHooks(OptionsSetupAction)
                .AddSingletonFilter<SlackVerifyTokenFilter>(WebHookSecurityFilter.Order);
        }

        /// <summary>
        /// Add Slack WebHook configuration and services to the specified <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to configure.</param>
        /// <param name="setupAction">
        /// An <see cref="Action{WebHookOptions}"/> to configure the provided <see cref="WebHookOptions"/>.
        /// </param>
        /// <returns>The <paramref name="builder"/>.</returns>
        public static IMvcCoreBuilder AddSlackWebHooks(
            this IMvcCoreBuilder builder,
            Action<WebHookOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            builder.AddSlackWebHooks();
            builder.Services.Configure(setupAction);

            return builder;
        }

        private static void SetupWebHookOptions(WebHookOptions options)
        {
            if (!options.HttpContextItemsTypes.Contains(typeof(NameValueCollection)))
            {
                options.HttpContextItemsTypes.Add(typeof(NameValueCollection));
            }
        }
    }
}
