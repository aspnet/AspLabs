// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Routing;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    /// <summary>
    /// An <see cref="IActionModelConvention"/> implementation that adds routing information
    /// (<see cref="SelectorModel"/> settings, including a template and constraints) to <see cref="ActionModel"/>s of
    /// WebHook actions.
    /// </summary>
    public class WebHookRoutingProvider : IApplicationModelProvider
    {
        private readonly WebHookReceiverExistsConstraint _existsConstraint;
        private readonly WebHookEventMapperConstraint _eventMapperConstraint;

        /// <summary>
        /// Instantiates a new <see cref="WebHookRoutingProvider"/> instance with the given
        /// <paramref name="existsConstraint"/> and <paramref name="eventMapperConstraint"/>.
        /// </summary>
        /// <param name="existsConstraint">The <see cref="WebHookReceiverExistsConstraint"/>.</param>
        /// <param name="eventMapperConstraint">The <see cref="WebHookEventMapperConstraint"/>.</param>
        public WebHookRoutingProvider(
            WebHookReceiverExistsConstraint existsConstraint,
            WebHookEventMapperConstraint eventMapperConstraint)
        {
            _existsConstraint = existsConstraint;
            _eventMapperConstraint = eventMapperConstraint;
        }

        /// <summary>
        /// Gets the <see cref="IApplicationModelProvider.Order"/> value used in all
        /// <see cref="WebHookRoutingProvider"/> instances. The WebHook <see cref="IApplicationModelProvider"/> order
        /// is
        /// <list type="number">
        /// <item>
        /// Add <see cref="IWebHookMetadata"/> references to the <see cref="ActionModel.Properties"/> collections of
        /// WebHook actions and validate those <see cref="IWebHookMetadata"/> attributes and services (in
        /// <see cref="WebHookMetadataProvider"/>).
        /// </item>
        /// <item>
        /// Add routing information (<see cref="SelectorModel"/> settings) to <see cref="ActionModel"/>s of WebHook
        /// actions (in this provider).
        /// </item>
        /// <item>
        /// Add filters to the <see cref="ActionModel.Filters"/> collections of WebHook actions (in
        /// <see cref="WebHookFilterProvider"/>).
        /// </item>
        /// <item>
        /// Add model binding information (<see cref="BindingInfo"/> settings) to <see cref="ParameterModel"/>s of
        /// WebHook actions (in <see cref="WebHookModelBindingProvider"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookMetadataProvider.Order + 10;

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

            var template = ChooseTemplate();
            var selectors = action.Selectors;
            if (selectors.Count == 0)
            {
                var selector = new SelectorModel();
                selectors.Add(selector);

                AddTemplate(attribute, template, selector);
            }
            else
            {
                for (var i = 0; i < selectors.Count; i++)
                {
                    var selector = selectors[i];
                    AddTemplate(attribute, template, selector);
                }
            }

            AddConstraints(attribute, selectors);
            AddConstraints(action.Properties, selectors);
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

        // Set the template for given SelectorModel. Similar to WebHookActionAttributeBase implementing
        // IRouteTemplateProvider.
        private static void AddTemplate(WebHookAttribute attribute, string template, SelectorModel selector)
        {
            if (selector.AttributeRouteModel?.Template != null)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.RoutingProvider_MixedRouteWithWebHookAttribute,
                    attribute.GetType(),
                    selector.AttributeRouteModel.Attribute?.GetType(),
                    attribute.GetType().Name);
                throw new InvalidOperationException(message);
            }

            if (selector.AttributeRouteModel == null)
            {
                selector.AttributeRouteModel = new AttributeRouteModel();
            }

            selector.AttributeRouteModel.Template = template;
        }

        private static void AddConstraint(IActionConstraintMetadata constraint, IList<SelectorModel> selectors)
        {
            for (var i = 0; i < selectors.Count; i++)
            {
                var selector = selectors[i];
                selector.ActionConstraints.Add(constraint);
            }
        }

        private void AddConstraints(WebHookAttribute attribute, IList<SelectorModel> selectors)
        {
            AddConstraint(_existsConstraint, selectors);

            if (attribute.ReceiverName != null)
            {
                var constraint = new WebHookReceiverNameConstraint(attribute.ReceiverName);
                AddConstraint(constraint, selectors);
            }

            if (attribute.Id != null)
            {
                var constraint = new WebHookIdConstraint(attribute.Id);
                AddConstraint(constraint, selectors);
            }
        }

        private void AddConstraints(IDictionary<object, object> properties, IList<SelectorModel> selectors)
        {
            if (properties.TryGetValue(typeof(IWebHookEventMetadata), out var eventMetadata))
            {
                AddConstraint(_eventMapperConstraint, selectors);
            }

            if (properties.TryGetValue(typeof(IWebHookEventSelectorMetadata), out var eventSourceMetadata))
            {
                var eventName = ((IWebHookEventSelectorMetadata)eventSourceMetadata).EventName;
                if (eventName != null)
                {
                    // IWebHookEventMetadata is mandatory when performing action selection using event names.
                    Debug.Assert(eventMetadata != null);
                    properties.TryGetValue(typeof(IWebHookPingRequestMetadata), out var pingMetadata);

                    // Use eventMetadata to choose constraint type because IWebHookPingRequestMetadata is optional.
                    IActionConstraintMetadata constraint;
                    if (eventMetadata is IWebHookEventMetadata)
                    {
                        constraint = new WebHookSingleEventNamesConstraint(
                            eventName,
                            ((IWebHookPingRequestMetadata)pingMetadata)?.PingEventName);
                    }
                    else
                    {
                        constraint = new WebHookMultipleEventNamesConstraint(
                            eventName,
                            (IReadOnlyList<IWebHookPingRequestMetadata>)pingMetadata);
                    }

                    AddConstraint(constraint, selectors);
                }
            }
        }
    }
}
