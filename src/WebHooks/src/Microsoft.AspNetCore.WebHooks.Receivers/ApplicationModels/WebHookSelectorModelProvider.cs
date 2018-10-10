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
using Microsoft.AspNetCore.WebHooks.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    /// <summary>
    /// An <see cref="IActionModelConvention"/> implementation that adds routing information
    /// (<see cref="SelectorModel"/> settings, including a template and constraints) to <see cref="ActionModel"/>s of
    /// WebHook actions.
    /// </summary>
    public class WebHookSelectorModelProvider : IApplicationModelProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly WebHookMetadataProvider _metadataProvider;

        /// <summary>
        /// Instantiates a new <see cref="WebHookSelectorModelProvider"/> instance.
        /// </summary>
        /// <param name="metadataProvider">The <see cref="WebHookMetadataProvider"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookSelectorModelProvider(
            WebHookMetadataProvider metadataProvider,
            ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _metadataProvider = metadataProvider;
        }

        /// <summary>
        /// Gets the <see cref="IApplicationModelProvider.Order"/> value used in all
        /// <see cref="WebHookSelectorModelProvider"/> instances. The WebHook <see cref="IApplicationModelProvider"/>
        /// order is
        /// <list type="number">
        /// <item>
        /// Add <see cref="IWebHookMetadata"/> references to the <see cref="ActionModel.Properties"/> collections of
        /// WebHook actions and validate those <see cref="IWebHookMetadata"/> attributes and services (in
        /// <see cref="WebHookActionModelPropertyProvider"/>).
        /// </item>
        /// <item>
        /// Add routing information (<see cref="SelectorModel"/> settings) to <see cref="ActionModel"/>s of WebHook
        /// actions (in this provider).
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
        public static int Order => WebHookActionModelPropertyProvider.Order + 10;

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

            // Check for conflicting attribute routing attributes and constraints. Ignore the empty SelectorModel that
            // DefaultApplicationModelProvider always creates.
            var selectors = action.Selectors
                .Concat(action.Controller.Selectors)
                .Where(selectorModel => selectorModel.ActionConstraints.Count != 0 ||
                    selectorModel.AttributeRouteModel != null)
                .ToArray();
            if (selectors.Length != 0)
            {
                // For the error message, prefer an IRouteTemplateProvider over a constraint that is an Attribute.
                // Prefer both over other constraints.
                var conflictingAttribute = selectors
                        .Select(model => (object)model.AttributeRouteModel?.Attribute)
                        .Concat(selectors.SelectMany(model => model.ActionConstraints.OfType<Attribute>()))
                        .Concat(selectors.SelectMany(model => model.ActionConstraints))
                        .FirstOrDefault();

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.SelectorModelProvider_MixedRouteWithWebHookAttribute,
                    attribute.GetType(),
                    conflictingAttribute?.GetType(),
                    attribute.GetType().Name);
                throw new InvalidOperationException(message);
            }

            // Set the template for given SelectorModel. Similar result to WebHookActionAttributeBase implementing
            // IRouteTemplateProvider.
            var selector = action.Selectors[0];
            selector.AttributeRouteModel = new AttributeRouteModel
            {
                Template = ChooseTemplate(),
            };

            var properties = action.Properties;
            AddEventNameMapperConstraint(properties, selector);
            AddEventNamesConstraint(properties, selector);
            AddIdConstraint(attribute, selector);
            AddReceiverNameConstraint(properties, selector);
        }

        // Use a constant template since all WebHook constraints use the resulting route values and we have no
        // requirements for user-specified route templates.
        private static string ChooseTemplate()
        {
            var template = "/api/webhooks/incoming/"
                + $"{{{WebHookConstants.ReceiverKeyName}}}/"
                + $"{{{WebHookConstants.IdKeyName}?}}";

            return template;
        }

        private void AddEventNameMapperConstraint(IDictionary<object, object> properties, SelectorModel selector)
        {
            if (properties.TryGetValue(typeof(IWebHookEventMetadata), out var eventMetadataObject))
            {
                WebHookEventNameMapperConstraint constraint;
                if (eventMetadataObject is IWebHookEventMetadata eventMetadata)
                {
                    constraint = new WebHookEventNameMapperConstraint(_loggerFactory, eventMetadata);
                }
                else
                {
                    constraint = new WebHookEventNameMapperConstraint(_loggerFactory, _metadataProvider);
                }

                selector.ActionConstraints.Add(constraint);
            }
        }

        private void AddEventNamesConstraint(IDictionary<object, object> properties, SelectorModel selector)
        {
            if (properties.TryGetValue(typeof(IWebHookEventSelectorMetadata), out var eventSourceMetadata))
            {
                var eventName = ((IWebHookEventSelectorMetadata)eventSourceMetadata).EventName;
                if (eventName != null)
                {
                    properties.TryGetValue(typeof(IWebHookPingRequestMetadata), out var pingRequestMetadataObject);

                    WebHookEventNameConstraint constraint;
                    if (pingRequestMetadataObject == null)
                    {
                        constraint = new WebHookEventNameConstraint(eventName);
                    }
                    else if (pingRequestMetadataObject is IWebHookPingRequestMetadata pingRequestMetadata)
                    {
                        constraint = new WebHookEventNameConstraint(eventName, pingRequestMetadata);
                    }
                    else
                    {
                        constraint = new WebHookEventNameConstraint(eventName, _metadataProvider);
                    }

                    selector.ActionConstraints.Add(constraint);
                }
            }
        }

        private void AddIdConstraint(WebHookAttribute attribute, SelectorModel selector)
        {
            if (attribute.Id != null)
            {
                var constraint = new WebHookIdConstraint(attribute.Id);
                selector.ActionConstraints.Add(constraint);
            }
        }

        private void AddReceiverNameConstraint(IDictionary<object, object> properties, SelectorModel selector)
        {
            var bodyTypeMetadataObject = properties[typeof(IWebHookBodyTypeMetadataService)];

            WebHookReceiverNameConstraint constraint;
            if (bodyTypeMetadataObject is IWebHookBodyTypeMetadataService bodyTypeMetadata)
            {
                constraint = new WebHookReceiverNameConstraint(bodyTypeMetadata);
            }
            else
            {
                constraint = new WebHookReceiverNameConstraint(_metadataProvider);
            }

            selector.ActionConstraints.Add(constraint);
        }
    }
}
