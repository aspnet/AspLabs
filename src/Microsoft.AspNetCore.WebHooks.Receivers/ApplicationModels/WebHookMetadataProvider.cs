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
        private readonly IReadOnlyList<IWebHookBodyTypeMetadataService> _bodyTypeMetadata;
        private readonly IReadOnlyList<IWebHookEventMetadata> _eventMetadata;
        private readonly IReadOnlyList<IWebHookPingRequestMetadata> _pingMetadata;
        private readonly ILogger _logger;

        /// <summary>
        /// Instantiates a new <see cref="WebHookMetadataProvider"/> with the given <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookMetadataProvider(IEnumerable<IWebHookMetadata> metadata, ILoggerFactory loggerFactory)
        {
            _bindingMetadata = metadata.OfType<IWebHookBindingMetadata>().ToArray();
            _bodyTypeMetadata = metadata.OfType<IWebHookBodyTypeMetadataService>().ToArray();
            _eventMetadata = metadata.OfType<IWebHookEventMetadata>().ToArray();
            _pingMetadata = metadata.OfType<IWebHookPingRequestMetadata>().ToArray();
            _logger = loggerFactory.CreateLogger<WebHookMetadataProvider>();

            // Check for duplicate registrations in the collections tracked here.
            EnsureUniqueRegistrations(_bindingMetadata);
            EnsureUniqueRegistrations(_bodyTypeMetadata);
            EnsureUniqueRegistrations(_eventMetadata);
            EnsureUniqueRegistrations(_pingMetadata);

            // Check for duplicates in other metadata registrations.
            var eventFromBodyMetadata = metadata.OfType<IWebHookEventFromBodyMetadata>().ToArray();
            EnsureUniqueRegistrations(eventFromBodyMetadata);
            EnsureUniqueRegistrations(metadata.OfType<IWebHookGetRequestMetadata>().ToArray());
            EnsureUniqueRegistrations(metadata.OfType<IWebHookVerifyCodeMetadata>().ToArray());

            // Check for IWebHookBodyTypeMetadata services that do not also implement IWebHookReceiver.
            EnsureValidBodyTypeMetadata(metadata);

            EnsureValidEventFromBodyMetadata(eventFromBodyMetadata, _bodyTypeMetadata);
            EnsureValidEventFromBodyMetadata(eventFromBodyMetadata, _eventMetadata);
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
                var bindingMetadata = _bindingMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (bindingMetadata != null)
                {
                    action.Properties[typeof(IWebHookBindingMetadata)] = bindingMetadata;
                }
            }

            IWebHookEventMetadata eventMetadata;
            if (receiverName == null)
            {
                // Pass along all IWebHookEventMetadata and IWebHookPingRequestMetadata instances.
                eventMetadata = null;
                action.Properties[typeof(IWebHookEventMetadata)] = _eventMetadata;
                action.Properties[typeof(IWebHookPingRequestMetadata)] = _pingMetadata;
            }
            else
            {
                eventMetadata = _eventMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (eventMetadata != null)
                {
                    action.Properties[typeof(IWebHookEventMetadata)] = eventMetadata;
                }

                var pingMetadata = _pingMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (pingMetadata != null)
                {
                    action.Properties[typeof(IWebHookPingRequestMetadata)] = pingMetadata;
                }
            }

            if (attribute is IWebHookEventSelectorMetadata eventSelector &&
                eventSelector.EventName != null)
            {
                if (eventMetadata == null && receiverName != null)
                {
                    // IWebHookEventMetadata is mandatory when performing action selection using event names.
                    _logger.LogCritical(
                        0,
                        "Invalid metadata services found for the '{ReceiverName}' WebHook receiver. Receivers with " +
                        "attributes implementing '{AttributeMetadataType}' must also provide a " +
                        "'{ServiceMetadataType}' service. Event selection is impossible otherwise.",
                        receiverName,
                        typeof(IWebHookEventSelectorMetadata),
                        typeof(IWebHookEventMetadata));

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.MetadataProvider_MissingMetadataServicesForReceiver,
                        receiverName,
                        typeof(IWebHookEventSelectorMetadata),
                        typeof(IWebHookEventMetadata));
                    throw new InvalidOperationException(message);
                }

                action.Properties[typeof(IWebHookEventSelectorMetadata)] = eventSelector;
            }

            // Find the request metadata. IWebHookBodyTypeMetadata is mandatory for every receiver.
            if (!(attribute is IWebHookBodyTypeMetadata bodyTypeMetadata))
            {
                if (receiverName == null)
                {
                    // Only the GeneralWebHookAttribute should have a null ReceiverName and it implements
                    // IWebHookBodyTypeMetadata.
                    var attributeTypeName = attribute.GetType().Name;
                    _logger.LogCritical(
                        1,
                        "'{AttributeType}' has a null {PropertyName} property but does not implement " +
                        "'{MetadataType}'.",
                        attributeTypeName,
                        nameof(attribute.ReceiverName),
                        typeof(IWebHookBodyTypeMetadata));

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.MetadataProvider_MissingAttributeMetadata,
                        attributeTypeName,
                        nameof(attribute.ReceiverName),
                        typeof(IWebHookBodyTypeMetadata));
                    throw new InvalidOperationException(message);
                }

                bodyTypeMetadata = _bodyTypeMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (bodyTypeMetadata == null)
                {
                    _logger.LogCritical(
                        2,
                        "No '{MetadataType}' implementation found for the '{ReceiverName}' WebHook receiver. Each " +
                        "receiver must register a '{ServiceMetadataType}' or provide a '{AttributeType}' subclass " +
                        "that implements '{MetadataType}'.",
                        typeof(IWebHookBodyTypeMetadata),
                        receiverName,
                        typeof(IWebHookBodyTypeMetadataService),
                        typeof(WebHookAttribute),
                        typeof(IWebHookBodyTypeMetadata));

                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.MetadataProvider_MissingMetadata,
                        typeof(IWebHookBodyTypeMetadata),
                        receiverName);
                    throw new InvalidOperationException(message);
                }
            }

            action.Properties[typeof(IWebHookBodyTypeMetadata)] = bodyTypeMetadata;
        }

        /// <summary>
        /// Ensure <see cref="IWebHookBodyTypeMetadata"/> registrations in given <paramref name="metadata"/> are valid.
        /// That is, confirm all such metadata also implements <see cref="IWebHookBodyTypeMetadataService"/>.
        /// </summary>
        /// <param name="metadata">The collection of <see cref="IWebHookMetadata"/> services.</param>
        protected void EnsureValidBodyTypeMetadata(IEnumerable<IWebHookMetadata> metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var nonServiceTypeNames = metadata
                .Where(item => item is IWebHookBodyTypeMetadata && !(item is IWebHookBodyTypeMetadataService))
                .Select(item => item.GetType().Name)
                .Distinct();

            var invalidRegistrations = false;
            foreach (var typeName in nonServiceTypeNames)
            {
                invalidRegistrations = true;
                _logger.LogCritical(
                    3,
                    "'{ConcreteType}' implements '{MetadataType}' but not '{ServiceType}'.",
                    typeName,
                    typeof(IWebHookBodyTypeMetadata),
                    typeof(IWebHookBodyTypeMetadataService));
            }

            if (invalidRegistrations)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_WrongInterface,
                    typeof(IWebHookBodyTypeMetadata),
                    typeof(IWebHookBodyTypeMetadataService));
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Ensure members of given <paramref name="eventFromBodyMetadata"/> collection are valid. That is, confirm
        /// the receivers also provide <see cref="IWebHookBodyTypeMetadataService"/> services.
        /// </summary>
        /// <param name="eventFromBodyMetadata">
        /// The collection of <see cref="IWebHookEventFromBodyMetadata"/> services.
        /// </param>
        /// <param name="bodyTypeMetadata">
        /// The collection of <see cref="IWebHookBodyTypeMetadataService"/> services.
        /// </param>
        protected void EnsureValidEventFromBodyMetadata(
            IReadOnlyList<IWebHookEventFromBodyMetadata> eventFromBodyMetadata,
            IReadOnlyList<IWebHookBodyTypeMetadataService> bodyTypeMetadata)
        {
            if (eventFromBodyMetadata == null)
            {
                throw new ArgumentNullException(nameof(eventFromBodyMetadata));
            }
            if (bodyTypeMetadata == null)
            {
                throw new ArgumentNullException(nameof(bodyTypeMetadata));
            }

            var invalidMetadata = false;
            var receiversMissingRequiredRequestMetadata = eventFromBodyMetadata
                .Where(metadata => !bodyTypeMetadata.Any(
                    innerMetadata => innerMetadata.IsApplicable(metadata.ReceiverName)))
                .Select(metadata => metadata.ReceiverName);
            foreach (var receiverName in receiversMissingRequiredRequestMetadata)
            {
                invalidMetadata = true;
                _logger.LogCritical(
                    4,
                    "The '{ReceiverName}' WebHook receiver has invalid '{MetadataType}'. Receivers must also have " +
                    "'{RequiredMetadataType}'.",
                    receiverName,
                    typeof(IWebHookEventFromBodyMetadata),
                    typeof(IWebHookBodyTypeMetadataService));
            }

            if (invalidMetadata)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_MissingMetadataServices,
                    typeof(IWebHookEventFromBodyMetadata),
                    typeof(IWebHookBodyTypeMetadataService));
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Ensure members of given <paramref name="eventFromBodyMetadata"/> collection are valid. That is, confirm
        /// no receiver provides both <see cref="IWebHookEventFromBodyMetadata"/> and
        /// <see cref="IWebHookEventMetadata"/> services.
        /// </summary>
        /// <param name="eventFromBodyMetadata">
        /// The collection of <see cref="IWebHookEventFromBodyMetadata"/> services.
        /// </param>
        /// <param name="eventMetadata">
        /// The collection of <see cref="IWebHookEventMetadata"/> services.
        /// </param>
        protected void EnsureValidEventFromBodyMetadata(
            IReadOnlyList<IWebHookEventFromBodyMetadata> eventFromBodyMetadata,
            IReadOnlyList<IWebHookEventMetadata> eventMetadata)
        {
            if (eventFromBodyMetadata == null)
            {
                throw new ArgumentNullException(nameof(eventFromBodyMetadata));
            }
            if (eventMetadata == null)
            {
                throw new ArgumentNullException(nameof(eventMetadata));
            }

            var invalidMetadata = false;
            var receiversWithConflictingMetadata = eventFromBodyMetadata
                .Where(metadata => eventMetadata.Any(
                    innerMetadata => innerMetadata.IsApplicable(metadata.ReceiverName)))
                .Select(metadata => metadata.ReceiverName);
            foreach (var receiverName in receiversWithConflictingMetadata)
            {
                invalidMetadata = true;
                _logger.LogCritical(
                    4,
                    "Invalid metadata services found for the '{ReceiverName}' WebHook receiver. Receivers must not " +
                    "provide both '{EventFromBodyMetadataType}' and '{EventMetadataType}' services.",
                    receiverName,
                    typeof(IWebHookEventFromBodyMetadata),
                    typeof(IWebHookEventMetadata));
            }

            if (invalidMetadata)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_ConflictingMetadataServices,
                    typeof(IWebHookEventFromBodyMetadata),
                    typeof(IWebHookEventMetadata));
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Ensure given <paramref name="services"/> collection does not contain duplicate registrations. That is,
        /// confirm the <typeparamref name="TService"/> registration for each
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
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var duplicateReceiverNames = services
                .GroupBy(item => item.ReceiverName, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() != 1)
                .Select(group => group.Key);

            var hasDuplicates = false;
            foreach (var receiverName in duplicateReceiverNames)
            {
                hasDuplicates = true;
                _logger.LogCritical(
                    5,
                    "Duplicate '{MetadataType}' registrations found for the '{ReceiverName}' WebHook receiver.",
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
