// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Base class for <see cref="IWebHookMetadata"/> services.
    /// </summary>
    public abstract class WebHookMetadata : IWebHookBodyTypeMetadataService
    {
        /// <summary>
        /// Instantiates a new <see cref="WebHookMetadata"/> instance with the given <paramref name="receiverName"/>.
        /// </summary>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        protected WebHookMetadata(string receiverName)
        {
            if (string.IsNullOrEmpty(receiverName))
            {
                throw new ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(receiverName));
            }

            ReceiverName = receiverName;
        }

        /// <inheritdoc />
        public abstract WebHookBodyType BodyType { get; }

        /// <inheritdoc />
        public string ReceiverName { get; }

        /// <inheritdoc />
        bool IWebHookReceiver.IsApplicable(string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            if (ReceiverName == null)
            {
                return true;
            }

            return string.Equals(ReceiverName, receiverName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Register <typeparamref name="TService"/> as all metadata interfaces it implements (always including
        /// <see cref="IWebHookBodyTypeMetadataService"/>) in <paramref name="services"/>.
        /// </summary>
        /// <typeparam name="TService">The <see cref="IWebHookMetadata"/> type to register.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to update.</param>
        public static void Register<TService>(IServiceCollection services)
            where TService : class, IWebHookBodyTypeMetadataService
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IWebHookBodyTypeMetadataService, TService>());

            var type = typeof(TService);
            if (typeof(IWebHookBindingMetadata).IsAssignableFrom(type))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IWebHookBindingMetadata), type));
            }

            if (typeof(IWebHookEventFromBodyMetadata).IsAssignableFrom(type))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IWebHookEventFromBodyMetadata), type));
            }

            if (typeof(IWebHookEventMetadata).IsAssignableFrom(type))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IWebHookEventMetadata), type));
            }

            if (typeof(IWebHookFilterMetadata).IsAssignableFrom(type))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IWebHookFilterMetadata), type));
            }

            if (typeof(IWebHookGetHeadRequestMetadata).IsAssignableFrom(type))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IWebHookGetHeadRequestMetadata), type));
            }

            if (typeof(IWebHookPingRequestMetadata).IsAssignableFrom(type))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IWebHookPingRequestMetadata), type));
            }

            if (typeof(IWebHookVerifyCodeMetadata).IsAssignableFrom(type))
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IWebHookVerifyCodeMetadata), type));
            }
        }
    }
}
