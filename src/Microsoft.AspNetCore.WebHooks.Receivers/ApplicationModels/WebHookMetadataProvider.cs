// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    /// <summary>
    /// <para>
    /// An <see cref="IApplicationModelProvider"/> implementation that adds <see cref="IWebHookMetadata"/>
    /// references to WebHook <see cref="ActionModel"/>s. Metadata is stored in <see cref="ActionModel.Properties"/>
    /// and used in <see cref="WebHookModelBindingProvider"/> and <see cref="WebHookRoutingProvider"/>.
    /// </para>
    /// <para>
    /// Detects missing and duplicate <see cref="IWebHookMetadata"/> services.
    /// </para>
    /// </summary>
    public class WebHookMetadataProvider : IApplicationModelProvider
    {
        private readonly IReadOnlyList<IWebHookBindingMetadata> _bindingMetadata;
        private readonly IReadOnlyList<IWebHookEventMetadata> _eventMetadata;
        private readonly IReadOnlyList<IWebHookRequestMetadataService> _requestMetadata;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="WebHookMetadataProvider"/> with the given <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookMetadataProvider(
            IEnumerable<IWebHookMetadata> metadata,
            ILoggerFactory loggerFactory)
        {
            _bindingMetadata = metadata.OfType<IWebHookBindingMetadata>().ToArray();
            _eventMetadata = metadata.OfType<IWebHookEventMetadata>().ToArray();
            _requestMetadata = metadata.OfType<IWebHookRequestMetadataService>().ToArray();

            _logger = loggerFactory.CreateLogger<WebHookMetadataProvider>();

            // Check for duplicate registrations for the collections tracked here.
            EnsureUniqueRegistrations(_bindingMetadata);
            EnsureUniqueRegistrations(_eventMetadata);
            EnsureUniqueRegistrations(_requestMetadata);

            // Check for duplicate IWebHookSecurityMetadata registrations.
            EnsureUniqueRegistrations(metadata.OfType<IWebHookSecurityMetadata>().ToArray());

            // Check for IWebHookRequestMetadata services that do not also implement IWebHookReceiver.
            var invalidRegistrations = false;
            var nonServiceTypeNames = metadata
                .Where(item => item is IWebHookRequestMetadata && !(item is IWebHookRequestMetadataService))
                .Select(item => item.GetType().Name)
                .Distinct();
            foreach (var typeName in nonServiceTypeNames)
            {
                invalidRegistrations = true;
                _logger.LogCritical(
                    0,
                    "'{ConcreteType}' implements '{MetadataType}' but not '{ServiceType}'.",
                    typeName,
                    typeof(IWebHookRequestMetadata),
                    typeof(IWebHookRequestMetadataService));
            }

            if (invalidRegistrations)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_WrongInterface,
                    typeof(IWebHookRequestMetadata),
                    typeof(IWebHookRequestMetadataService));
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Gets the <see cref="IApplicationModelProvider.Order"/> value used in all
        /// <see cref="WebHookMetadataProvider"/> instances.
        /// </summary>
        /// <value>
        /// Chosen to ensure this provider runs after MVC's <see cref="Mvc.Internal.DefaultApplicationModelProvider"/>.
        /// </value>
        public static int Order => -500;

        /// <inheritdoc />
        int IApplicationModelProvider.Order => Order;

        /// <inheritdoc />
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            for (var i = 0; i < context.Result.Controllers.Count; i++)
            {
                var controller = context.Result.Controllers[i];
                for (var j = 0; j < controller.Actions.Count; j++)
                {
                    var action = controller.Actions[j];
                    Apply(action);
                }
            }
        }

        /// <inheritdoc />
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            // No-op
        }

        private void Apply(ActionModel action)
        {
            var attribute = action.Attributes.OfType<WebHookAttribute>().FirstOrDefault();
            if (attribute == null)
            {
                // Not a WebHook handler.
                return;
            }

            var receiverName = attribute.ReceiverName;
            if (receiverName != null)
            {
                var bindingMMetadata = _bindingMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (bindingMMetadata != null)
                {
                    action.Properties[typeof(IWebHookBindingMetadata)] = bindingMMetadata;
                }
            }

            IWebHookEventMetadata eventMetadata;
            if (receiverName == null)
            {
                // Pass along all IWebHookEventMetadata instances.
                eventMetadata = null;
                action.Properties[typeof(IWebHookEventMetadata)] = _eventMetadata;
            }
            else
            {
                eventMetadata = _eventMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (eventMetadata != null)
                {
                    action.Properties[typeof(IWebHookEventMetadata)] = eventMetadata;
                }
            }

            if (attribute is IWebHookEventSelectorMetadata eventSelector &&
                eventSelector.EventName != null)
            {
                if (eventMetadata == null && receiverName != null)
                {
                    // IWebHookEventMetadata is mandatory when performing action selection using event names.
                    _logger.LogCritical(
                        1,
                        "No '{MetadataType}' implementation found for the '{ReceiverName}' receiver. Event " +
                        "selection is impossible in this case.",
                        typeof(IWebHookEventMetadata),
                        receiverName);

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.MetadataProvider_MissingMetadata,
                        typeof(IWebHookEventMetadata),
                        receiverName);
                    throw new InvalidOperationException(message);
                }

                action.Properties[typeof(IWebHookEventSelectorMetadata)] = eventSelector;
            }

            // Find the request metadata. IWebHookRequestMetadata is mandatory for every receiver.
            if (!(attribute is IWebHookRequestMetadata requestMetadata))
            {
                if (receiverName == null)
                {
                    // Only the GeneralWebHookAttribute should have a null ReceiverName and it implements
                    // IWebHookRequestMetadata.
                    var attributeTypeName = attribute.GetType().Name;
                    _logger.LogCritical(
                        2,
                        "'{ConcreteType}' has a null {PropertyName} property but does not implement '{MetadataType}'.",
                        attributeTypeName,
                        nameof(attribute.ReceiverName),
                        typeof(IWebHookRequestMetadata));

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.MetadataProvider_MissingAttributeMetadata,
                        attributeTypeName,
                        nameof(attribute.ReceiverName),
                        typeof(IWebHookRequestMetadata));
                    throw new InvalidOperationException(message);
                }

                requestMetadata = _requestMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (requestMetadata == null)
                {
                    _logger.LogCritical(
                        3,
                        "No '{MetadataType}' implementation found for the '{ReceiverName}' receiver. Each receiver " +
                        "must register a '{ServiceMetadataType}' or provide a '{AttributeType}' subclass that " +
                        "implements '{MetadataType}'.",
                        typeof(IWebHookRequestMetadata),
                        receiverName,
                        typeof(IWebHookRequestMetadataService),
                        typeof(WebHookAttribute),
                        typeof(IWebHookRequestMetadata));

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.MetadataProvider_MissingMetadata,
                        typeof(IWebHookRequestMetadata),
                        receiverName);
                    throw new InvalidOperationException(message);
                }
            }

            action.Properties[typeof(IWebHookRequestMetadata)] = requestMetadata;
        }

        /// <summary>
        /// Ensures given <paramref name="services"/> collection does not contain duplicate registrations. That is,
        /// confirms the <typeparamref name="TService"/> registration for each
        /// <see cref="IWebHookReceiver.ReceiverName"/> is unique.
        /// </summary>
        /// <typeparam name="TService">
        /// The <see cref="IWebHookReceiver"/> interface of the <paramref name="services"/> to check.
        /// </typeparam>
        /// <param name="services">The collection of <typeparamref name="TService"/> services to check.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if duplicates exist in <paramref name="services"/>.
        /// </exception>
        protected void EnsureUniqueRegistrations<TService>(IReadOnlyList<TService> services)
            where TService : IWebHookReceiver
        {
            var hasDuplicates = false;
            var duplicateReceiverNames = services
                .GroupBy(item => item.ReceiverName, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() != 1)
                .Select(group => group.Key);

            foreach (var receiverName in duplicateReceiverNames)
            {
                hasDuplicates = true;
                _logger.LogCritical(
                    4,
                    "Duplicate '{MetadataType}' registrations found for the '{ReceiverName}' receiver.",
                    typeof(TService),
                    receiverName);
            }

            if (hasDuplicates)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_DuplicateMetadata,
                    typeof(TService));
                throw new InvalidOperationException(message);
            }
        }
    }
}
