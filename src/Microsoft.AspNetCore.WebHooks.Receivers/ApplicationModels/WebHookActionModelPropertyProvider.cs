// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    /// <summary>
    /// <para>
    /// An <see cref="IApplicationModelProvider"/> implementation that adds <see cref="IWebHookMetadata"/>
    /// references to <see cref="ActionModel.Properties"/> collections of WebHook actions. Later WebHook
    /// <see cref="IApplicationModelProvider"/> implementations (<see cref="WebHookActionModelFilterProvider"/>,
    /// <see cref="WebHookBindingInfoProvider"/> and <see cref="WebHookSelectorModelProvider"/>) use this metadata.
    /// </para>
    /// <para>
    /// Detects duplicate, missing and invalid <see cref="IWebHookMetadata"/> attributes and services.
    /// </para>
    /// </summary>
    public class WebHookActionModelPropertyProvider : IApplicationModelProvider
    {
        private readonly IReadOnlyList<IWebHookBindingMetadata> _bindingMetadata;
        private readonly IReadOnlyList<IWebHookBodyTypeMetadataService> _bodyTypeMetadata;
        private readonly IReadOnlyList<IWebHookEventFromBodyMetadata> _eventFromBodyMetadata;
        private readonly IReadOnlyList<IWebHookEventMetadata> _eventMetadata;
        private readonly IReadOnlyList<IWebHookFilterMetadata> _filterMetadata;
        private readonly IReadOnlyList<IWebHookGetHeadRequestMetadata> _getHeadRequestMetadata;
        private readonly IReadOnlyList<IWebHookPingRequestMetadata> _pingRequestMetadata;
        private readonly IReadOnlyList<IWebHookVerifyCodeMetadata> _verifyCodeMetadata;

        /// <summary>
        /// Instantiates a new <see cref="WebHookActionModelPropertyProvider"/> instance with the given metadata.
        /// </summary>
        /// <param name="bindingMetadata">The collection of <see cref="IWebHookBindingMetadata"/> services.</param>
        /// <param name="bodyTypeMetadata">
        /// The collection of <see cref="IWebHookBodyTypeMetadataService"/> services.
        /// </param>
        /// <param name="eventFromBodyMetadata">
        /// The collection of <see cref="IWebHookEventFromBodyMetadata"/> services.
        /// </param>
        /// <param name="eventMetadata">The collection of <see cref="IWebHookEventMetadata"/> services.</param>
        /// <param name="filterMetadata">The collection of <see cref="IWebHookFilterMetadata"/> services.</param>
        /// <param name="getHeadRequestMetadata">
        /// The collection of <see cref="IWebHookGetHeadRequestMetadata"/> services.
        /// </param>
        /// <param name="pingRequestMetadata">
        /// The collection of <see cref="IWebHookPingRequestMetadata"/> services.
        /// </param>
        /// <param name="verifyCodeMetadata">
        /// The collection of <see cref="IWebHookVerifyCodeMetadata"/> services.
        /// </param>
        public WebHookActionModelPropertyProvider(
            IEnumerable<IWebHookBindingMetadata> bindingMetadata,
            IEnumerable<IWebHookBodyTypeMetadataService> bodyTypeMetadata,
            IEnumerable<IWebHookEventFromBodyMetadata> eventFromBodyMetadata,
            IEnumerable<IWebHookEventMetadata> eventMetadata,
            IEnumerable<IWebHookFilterMetadata> filterMetadata,
            IEnumerable<IWebHookGetHeadRequestMetadata> getHeadRequestMetadata,
            IEnumerable<IWebHookPingRequestMetadata> pingRequestMetadata,
            IEnumerable<IWebHookVerifyCodeMetadata> verifyCodeMetadata)
        {
            _bindingMetadata = bindingMetadata.ToArray();
            _bodyTypeMetadata = bodyTypeMetadata.ToArray();
            _eventFromBodyMetadata = eventFromBodyMetadata.ToArray();
            _eventMetadata = eventMetadata.ToArray();
            _filterMetadata = filterMetadata.ToArray();
            _getHeadRequestMetadata = getHeadRequestMetadata.ToArray();
            _pingRequestMetadata = pingRequestMetadata.ToArray();
            _verifyCodeMetadata = verifyCodeMetadata.ToArray();

            // Check for duplicate registrations in the collections tracked here.
            EnsureUniqueRegistrations(_bindingMetadata);
            EnsureUniqueRegistrations(_bodyTypeMetadata);
            EnsureUniqueRegistrations(_eventFromBodyMetadata);
            EnsureUniqueRegistrations(_eventMetadata);
            EnsureUniqueRegistrations(_filterMetadata);
            EnsureUniqueRegistrations(_getHeadRequestMetadata);
            EnsureUniqueRegistrations(_pingRequestMetadata);
            EnsureUniqueRegistrations(_verifyCodeMetadata);

            EnsureValidBodyTypeMetadata(_bodyTypeMetadata);
            EnsureValidEventFromBodyMetadata(_eventFromBodyMetadata, _eventMetadata);

            // Confirm no incomplete receivers exist i.e. no metadata services exist for a receiver unless one
            // implements IWebHookBodyTypeMetadataService.
            var receiverNames = _bindingMetadata
                .Cast<IWebHookReceiver>()
                .Concat(_eventFromBodyMetadata)
                .Concat(_eventMetadata)
                .Concat(_filterMetadata)
                .Concat(_getHeadRequestMetadata)
                .Concat(_pingRequestMetadata)
                .Concat(_verifyCodeMetadata)
                .Select(metadata => metadata.ReceiverName)
                .Distinct(StringComparer.OrdinalIgnoreCase);
            foreach (var receiverName in receiverNames)
            {
                var bodyTypeMetadataService = _bodyTypeMetadata
                    .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                EnsureValidBodyTypeMetadata(bodyTypeMetadataService, receiverName);
            }
        }

        /// <summary>
        /// Gets the <see cref="IApplicationModelProvider.Order"/> value used in all
        /// <see cref="WebHookActionModelPropertyProvider"/> instances. The WebHook
        /// <see cref="IApplicationModelProvider"/> order is
        /// <list type="number">
        /// <item>
        /// Add <see cref="IWebHookMetadata"/> references to the <see cref="ActionModel.Properties"/> collections of
        /// WebHook actions and validate those <see cref="IWebHookMetadata"/> attributes and services (in this
        /// provider).
        /// </item>
        /// <item>
        /// Add routing information (<see cref="SelectorModel"/> settings) to <see cref="ActionModel"/>s of WebHook
        /// actions (in <see cref="WebHookSelectorModelProvider"/>).
        /// </item>
        /// <item>
        /// Add filters to the <see cref="ActionModel.Filters"/> collections of WebHook actions (in
        /// <see cref="WebHookActionModelFilterProvider"/>).
        /// </item>
        /// <item>
        /// Add model binding information (<see cref="BindingInfo"/> settings) to <see cref="ParameterModel"/>s of
        /// WebHook actions (in <see cref="WebHookBindingInfoProvider"/>).
        /// </item>
        /// </list>
        /// </summary>
        /// <value>
        /// Chosen to ensure WebHook providers run after MVC's
        /// <see cref="Mvc.Internal.DefaultApplicationModelProvider"/>.
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
            if (receiverName == null)
            {
                // Pass along most IWebHook*Metadata instances. No need for the IWebHookFilterMetadata collection
                // because WebHookFilterProvider is always registered.
                if (_bindingMetadata.Count != 0)
                {
                    action.Properties[typeof(IWebHookBindingMetadata)] = _bindingMetadata;
                }

                if (_eventFromBodyMetadata.Count != 0)
                {
                    action.Properties[typeof(IWebHookEventFromBodyMetadata)] = _eventFromBodyMetadata;
                }

                if (_getHeadRequestMetadata.Count != 0)
                {
                    action.Properties[typeof(IWebHookGetHeadRequestMetadata)] = _getHeadRequestMetadata;
                }

                if (_pingRequestMetadata.Count != 0)
                {
                    action.Properties[typeof(IWebHookPingRequestMetadata)] = _pingRequestMetadata;
                }

                if (_verifyCodeMetadata.Count != 0)
                {
                    action.Properties[typeof(IWebHookVerifyCodeMetadata)] = _verifyCodeMetadata;
                }
            }
            else
            {
                var bindingMetadata = _bindingMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (bindingMetadata != null)
                {
                    action.Properties[typeof(IWebHookBindingMetadata)] = bindingMetadata;
                }

                var eventFromBodyMetadata = _eventFromBodyMetadata
                    .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (eventFromBodyMetadata != null)
                {
                    action.Properties[typeof(IWebHookEventFromBodyMetadata)] = eventFromBodyMetadata;
                }

                var filterMetadata = _filterMetadata.FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (filterMetadata != null)
                {
                    action.Properties[typeof(IWebHookFilterMetadata)] = filterMetadata;
                }

                var getHeadRequestMetadata = _getHeadRequestMetadata
                    .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (getHeadRequestMetadata != null)
                {
                    action.Properties[typeof(IWebHookGetHeadRequestMetadata)] = getHeadRequestMetadata;
                }

                var pingRequestMetadata = _pingRequestMetadata
                    .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (pingRequestMetadata != null)
                {
                    action.Properties[typeof(IWebHookPingRequestMetadata)] = pingRequestMetadata;
                }

                var verifyCodeMetadata = _verifyCodeMetadata
                    .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                if (verifyCodeMetadata != null)
                {
                    action.Properties[typeof(IWebHookVerifyCodeMetadata)] = verifyCodeMetadata;
                }
            }

            IWebHookEventMetadata eventMetadata;
            if (receiverName == null)
            {
                eventMetadata = null;
                if (_eventMetadata.Count != 0)
                {
                    action.Properties[typeof(IWebHookEventMetadata)] = _eventMetadata;
                }
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
                EnsureValidEventMetadata(eventMetadata, receiverName);
                action.Properties[typeof(IWebHookEventSelectorMetadata)] = eventSelector;
            }

            if (receiverName == null)
            {
                // WebHookVerifyBodyTypeFilter should look up (and verify) the applicable
                // IWebHookBodyTypeMetadataService per-request.
                action.Properties[typeof(IWebHookBodyTypeMetadataService)] = _bodyTypeMetadata;
            }
            else
            {
                var bodyTypeMetadata = _bodyTypeMetadata
                    .FirstOrDefault(metadata => metadata.IsApplicable(receiverName));
                EnsureValidBodyTypeMetadata(bodyTypeMetadata, receiverName);
                action.Properties[typeof(IWebHookBodyTypeMetadataService)] = bodyTypeMetadata;
            }

            // Ignore IWebHookBodyTypeMetadata metadata if attribute's BodyTypes property is null or if an attribute
            // other than GeneralWebHookAttribute implements this interface.
            if (receiverName == null &&
                attribute is IWebHookBodyTypeMetadata actionBodyTypeMetadata &&
                actionBodyTypeMetadata.BodyType.HasValue)
            {
                EnsureValidBodyTypeMetadata(actionBodyTypeMetadata);
                action.Properties[typeof(IWebHookBodyTypeMetadata)] = actionBodyTypeMetadata;
            }
        }

        /// <summary>
        /// Ensure members of given <paramref name="bodyTypeMetadataServices"/> collection are valid. That is, ensure
        /// each has a valid <see cref="IWebHookBodyTypeMetadataService.BodyType"/>.
        /// </summary>
        /// <param name="bodyTypeMetadataServices">
        /// The collection of <see cref="IWebHookBodyTypeMetadataService"/> services.
        /// </param>
        protected void EnsureValidBodyTypeMetadata(
            IReadOnlyList<IWebHookBodyTypeMetadataService> bodyTypeMetadataServices)
        {
            if (bodyTypeMetadataServices == null)
            {
                throw new ArgumentNullException(nameof(bodyTypeMetadataServices));
            }

            foreach (var bodyTypeMetadata in bodyTypeMetadataServices)
            {
                if (!Enum.IsDefined(typeof(WebHookBodyType), bodyTypeMetadata.BodyType))
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.General_InvalidEnumValue,
                        typeof(WebHookBodyType),
                        bodyTypeMetadata.BodyType);
                    throw new InvalidOperationException(message);
                }
            }
        }

        /// <summary>
        /// Ensure given <paramref name="bodyTypeMetadata"/> is valid.
        /// </summary>
        /// <param name="bodyTypeMetadata">
        /// An attribute that implements <see cref="IWebHookBodyTypeMetadata"/>.
        /// </param>
        protected void EnsureValidBodyTypeMetadata(IWebHookBodyTypeMetadata bodyTypeMetadata)
        {
            if (bodyTypeMetadata == null)
            {
                throw new ArgumentNullException(nameof(bodyTypeMetadata));
            }

            if (!bodyTypeMetadata.BodyType.HasValue)
            {
                return;
            }

            if (!Enum.IsDefined(typeof(WebHookBodyType), bodyTypeMetadata.BodyType.Value))
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.General_InvalidEnumValue,
                    typeof(WebHookBodyType),
                    bodyTypeMetadata.BodyType.Value);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Ensure given <paramref name="bodyTypeMetadata"/> is not <see langword="null"/>.
        /// An <see cref="IWebHookBodyTypeMetadataService"/> service is mandatory for every receiver.
        /// </summary>
        /// <param name="bodyTypeMetadata">
        /// The <paramref name="receiverName"/> receiver's <see cref="IWebHookBodyTypeMetadataService"/>, if any.
        /// </param>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        /// <remarks>
        /// Called to detect both incomplete receiver metadata and receiver-specific attributes for which no metadata
        /// has been registered.
        /// </remarks>
        protected void EnsureValidBodyTypeMetadata(
            IWebHookBodyTypeMetadataService bodyTypeMetadata,
            string receiverName)
        {
            if (receiverName == null)
            {
                throw new ArgumentNullException(nameof(receiverName));
            }

            if (bodyTypeMetadata == null)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_MissingMetadata,
                    receiverName,
                    typeof(IWebHookBodyTypeMetadataService));
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>
        /// Ensure given <paramref name="eventMetadata"/> is not <see langword="null"/>. An
        /// <see cref="IWebHookEventMetadata"/> service is mandatory for receivers with an attribute that implements
        /// <see cref="IWebHookEventSelectorMetadata"/>.
        /// </summary>
        /// <param name="eventMetadata">
        /// The <paramref name="receiverName"/> receiver's <see cref="IWebHookEventMetadata"/>, if any.
        /// </param>
        /// <param name="receiverName">The name of an available <see cref="IWebHookReceiver"/>.</param>
        protected void EnsureValidEventMetadata(IWebHookEventMetadata eventMetadata, string receiverName)
        {
            if (receiverName == null)
            {
                // Unusual case likely involves a GeneralWebHookAttribute subclass that implements
                // IWebHookEventSelectorMetadata. Assume developer adds runtime checks for IWebHookEventMetadata.
                return;
            }

            if (eventMetadata == null)
            {
                // IWebHookEventMetadata is mandatory when performing action selection using event names.
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_MissingMetadataServices,
                    receiverName,
                    typeof(IWebHookEventSelectorMetadata),
                    typeof(IWebHookEventMetadata));
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

            var receiverWithConflictingMetadata = eventFromBodyMetadata
                .FirstOrDefault(metadata => eventMetadata.Any(
                    innerMetadata => innerMetadata.IsApplicable(metadata.ReceiverName)));
            if (receiverWithConflictingMetadata != null)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_ConflictingMetadataServices,
                    receiverWithConflictingMetadata.ReceiverName,
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

            var duplicateMetadataGroup = services
                .GroupBy(item => item.ReceiverName, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() != 1);
            if (duplicateMetadataGroup != null)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.MetadataProvider_DuplicateMetadata,
                    duplicateMetadataGroup.Key, // ReceiverName
                    typeof(TService));
                throw new InvalidOperationException(message);
            }
        }
    }
}
