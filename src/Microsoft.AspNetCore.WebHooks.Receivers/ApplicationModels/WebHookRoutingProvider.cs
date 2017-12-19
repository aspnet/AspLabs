// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Routing;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    /// <summary>
    /// An <see cref="IActionModelConvention"/> implementation that adds attribute routing information to WebHook
    /// actions.
    /// </summary>
    public class WebHookRoutingProvider : IApplicationModelProvider
    {
        private readonly WebHookReceiverExistsConstraint _existsConstraint;
        private readonly WebHookEventMapperConstraint _eventMapperConstraint;
        private readonly ILoggerFactory _loggerFactory;

        // ??? Should we use constraint factories to let DI choose the constraint lifetimes?
        /// <summary>
        /// Instantiates a new <see cref="WebHookRoutingProvider"/> with the given
        /// <paramref name="existsConstraint"/>, <paramref name="eventMapperConstraint"/> and
        /// <paramref name="loggerFactory"/>.
        /// </summary>
        /// <param name="existsConstraint">The <see cref="WebHookReceiverExistsConstraint"/>.</param>
        /// <param name="eventMapperConstraint">The <see cref="WebHookEventMapperConstraint"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookRoutingProvider(
            WebHookReceiverExistsConstraint existsConstraint,
            WebHookEventMapperConstraint eventMapperConstraint,
            ILoggerFactory loggerFactory)
        {
            _existsConstraint = existsConstraint;
            _eventMapperConstraint = eventMapperConstraint;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public int Order => WebHookMetadataProvider.Order + 10;

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

        /// <inheritdoc />
        public void Apply(ActionModel action)
        {
            var attribute = action.Attributes.OfType<WebHookAttribute>().FirstOrDefault();
            if (attribute == null)
            {
                // Not a WebHook handler.
                return;
            }

            var template = ChooseTemplate(action.RouteValues);
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

            if (action.Properties.TryGetValue(typeof(IWebHookBodyTypeMetadata), out var bodyTypeMetadata))
            {
                action.Filters.Add(new WebHookVerifyBodyTypeFilter(
                    (IWebHookBodyTypeMetadata)bodyTypeMetadata,
                    _loggerFactory));
            }
        }

        // Use a constant template since we'll need constraints in any case. That is, need constraints either to match
        // receiver names and ids (current choice) or need them to map another route value to what model binding expects.
        private static string ChooseTemplate(IDictionary<string, string> routeValues)
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
                    Resources.RoutingConvention_MixedRouteWithWebHookAttribute,
                    attribute.GetType().Name,
                    selector.AttributeRouteModel.Attribute?.GetType().Name);
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
