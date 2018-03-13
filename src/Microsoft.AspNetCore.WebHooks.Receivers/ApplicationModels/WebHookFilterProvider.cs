// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    /// <summary>
    /// An <see cref="IApplicationModelProvider"/> implementation that adds a <see cref="WebHookVerifyBodyTypeFilter"/>
    /// and a <see cref="ModelStateInvalidFilter"/> (unless
    /// <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/> is <see langword="true"/>) filter to the
    /// <see cref="ActionModel.Filters"/> collections of WebHook actions.
    /// </summary>
    public class WebHookFilterProvider : IApplicationModelProvider
    {
        private readonly ApiBehaviorOptions _behaviorOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ModelStateInvalidFilter _modelStateInvalidFilter;

        /// <summary>
        /// Instantiates a new <see cref="WebHookFilterProvider"/> instance with the given
        /// <paramref name="behaviorOptions"/> and <paramref name="loggerFactory"/>.
        /// </summary>
        /// <param name="behaviorOptions">The <see cref="ApiBehaviorOptions"/> accessor.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public WebHookFilterProvider(IOptions<ApiBehaviorOptions> behaviorOptions, ILoggerFactory loggerFactory)
        {
            _behaviorOptions = behaviorOptions.Value;
            _loggerFactory = loggerFactory;

            var logger = loggerFactory.CreateLogger<ModelStateInvalidFilter>();
            _modelStateInvalidFilter = new ModelStateInvalidFilter(_behaviorOptions, logger);
        }

        /// <summary>
        /// Gets the <see cref="IApplicationModelProvider.Order"/> value used in all
        /// <see cref="WebHookFilterProvider"/> instances. The WebHook <see cref="IApplicationModelProvider"/> order
        /// is
        /// <list type="number">
        /// <item>
        /// Add <see cref="IWebHookMetadata"/> references to the <see cref="ActionModel.Properties"/> collections of
        /// WebHook actions and validate those <see cref="IWebHookMetadata"/> attributes and services (in
        /// <see cref="WebHookMetadataProvider"/>).
        /// </item>
        /// <item>
        /// Add routing information (<see cref="SelectorModel"/> settings) to <see cref="ActionModel"/>s of WebHook
        /// actions (in <see cref="WebHookRoutingProvider"/>).
        /// </item>
        /// <item>
        /// Add filters to the <see cref="ActionModel.Filters"/> collections of WebHook actions (in this provider).
        /// </item>
        /// <item>
        /// Add model binding information (<see cref="BindingInfo"/> settings) to <see cref="ParameterModel"/>s of
        /// WebHook actions (in <see cref="WebHookModelBindingProvider"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookRoutingProvider.Order + 10;

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

            var filters = action.Filters;
            AddVerifyBodyFilter(action.Properties, filters);
            if (!_behaviorOptions.SuppressModelStateInvalidFilter)
            {
                filters.Add(_modelStateInvalidFilter);
            }
        }

        private void AddVerifyBodyFilter(IDictionary<object, object> properties, IList<IFilterMetadata> filters)
        {
            WebHookVerifyBodyTypeFilter filter;
            var bodyTypeMetadataObject = properties[typeof(IWebHookBodyTypeMetadataService)];
            if (bodyTypeMetadataObject is IWebHookBodyTypeMetadataService receiverBodyTypeMetadata)
            {
                filter = new WebHookVerifyBodyTypeFilter(receiverBodyTypeMetadata, _loggerFactory);
            }
            else
            {
                var allBodyTypeMetadata = (IReadOnlyList<IWebHookBodyTypeMetadataService>)bodyTypeMetadataObject;
                filter = new WebHookVerifyBodyTypeFilter(allBodyTypeMetadata, _loggerFactory);
            }

            filters.Add(filter);
        }
    }
}
