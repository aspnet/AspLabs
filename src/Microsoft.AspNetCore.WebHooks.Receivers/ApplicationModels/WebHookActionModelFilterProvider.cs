// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Filters;
using Microsoft.AspNetCore.WebHooks.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.WebHooks.ApplicationModels
{
    /// <summary>
    /// An <see cref="IApplicationModelProvider"/> implementation that adds WebHook <see cref="IFilterMetadata"/>
    /// implementations and a <see cref="ModelStateInvalidFilter"/> (unless
    /// <see cref="ApiBehaviorOptions.SuppressModelStateInvalidFilter"/> is <see langword="true"/>) to the
    /// <see cref="ActionModel.Filters"/> collections of WebHook actions.
    /// </summary>
    public class WebHookActionModelFilterProvider : IApplicationModelProvider
    {
        private readonly ApiBehaviorOptions _behaviorOptions;
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILoggerFactory _loggerFactory;
        private readonly WebHookMetadataProvider _metadataProvider;
        private readonly ModelStateInvalidFilter _modelStateInvalidFilter;
        private readonly IWebHookRequestReader _requestReader;
        private readonly WebHookVerifyMethodFilter _verifyMethodFilter;

        /// <summary>
        /// Instantiates a new <see cref="WebHookActionModelFilterProvider"/> instance.
        /// </summary>
        /// <param name="behaviorOptions">The <see cref="ApiBehaviorOptions"/> accessor.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment" />.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadataProvider">
        /// The <see cref="WebHookMetadataProvider"/> service. Searched for applicable metadata per-request.
        /// </param>
        /// <param name="requestReader">The <see cref="IWebHookRequestReader"/>.</param>
        /// <param name="verifyMethodFilter">The <see cref="WebHookVerifyMethodFilter"/> service.</param>
        public WebHookActionModelFilterProvider(
            IOptions<ApiBehaviorOptions> behaviorOptions,
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment,
            ILoggerFactory loggerFactory,
            WebHookMetadataProvider metadataProvider,
            IWebHookRequestReader requestReader,
            WebHookVerifyMethodFilter verifyMethodFilter)
        {
            _behaviorOptions = behaviorOptions.Value;
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _loggerFactory = loggerFactory;
            _metadataProvider = metadataProvider;
            _requestReader = requestReader;
            _verifyMethodFilter = verifyMethodFilter;

            var logger = loggerFactory.CreateLogger<ModelStateInvalidFilter>();
            _modelStateInvalidFilter = new ModelStateInvalidFilter(_behaviorOptions, logger);
        }

        /// <summary>
        /// Gets the <see cref="IApplicationModelProvider.Order"/> value used in all
        /// <see cref="WebHookActionModelFilterProvider"/> instances. The WebHook
        /// <see cref="IApplicationModelProvider"/> order is
        /// <list type="number">
        /// <item>
        /// Add <see cref="IWebHookMetadata"/> references to the <see cref="ActionModel.Properties"/> collections of
        /// WebHook actions and validate those <see cref="IWebHookMetadata"/> attributes and services (in
        /// <see cref="WebHookActionModelPropertyProvider"/>).
        /// </item>
        /// <item>
        /// Add routing information (<see cref="SelectorModel"/> settings) to <see cref="ActionModel"/>s of WebHook
        /// actions (in <see cref="WebHookSelectorModelProvider"/>).
        /// </item>
        /// <item>
        /// Add filters to the <see cref="ActionModel.Filters"/> collections of WebHook actions (in this provider).
        /// </item>
        /// <item>
        /// Add model binding information (<see cref="BindingInfo"/> settings) to <see cref="ParameterModel"/>s of
        /// WebHook actions (in <see cref="WebHookBindingInfoProvider"/>).
        /// </item>
        /// </list>
        /// </summary>
        public static int Order => WebHookSelectorModelProvider.Order + 10;

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
            AddReceiverFilters(action, filters);
            if (!_behaviorOptions.SuppressModelStateInvalidFilter)
            {
                filters.Add(_modelStateInvalidFilter);
            }

            var properties = action.Properties;
            AddEventNameMapperFilter(properties, filters);
            AddGetHeadRequestFilter(properties, filters);
            AddPingRequestFilter(properties, filters);
            AddVerifyBodyTypeFilter(properties, filters);
            AddVerifyCodeFilter(properties, filters);
            filters.Add(_verifyMethodFilter);
            AddVerifyRequiredValueFilter(properties, filters);
        }

        private void AddReceiverFilters(ActionModel action, IList<IFilterMetadata> filters)
        {
            if (action.Properties.TryGetValue(typeof(IWebHookFilterMetadata), out var filterMetadataObject) &&
                filterMetadataObject is IWebHookFilterMetadata filterMetadata)
            {
                var context = new WebHookFilterMetadataContext(action);
                filterMetadata.AddFilters(context);
                foreach (var filter in context.Results)
                {
                    filters.Add(filter);
                }
            }
        }

        private void AddEventNameMapperFilter(IDictionary<object, object> properties, IList<IFilterMetadata> filters)
        {
            if (properties.TryGetValue(typeof(IWebHookEventFromBodyMetadata), out var eventFromBodyMetadataObject))
            {
                WebHookEventNameMapperFilter filter;
                var bodyTypeMetadataObject = properties[typeof(IWebHookBodyTypeMetadataService)];
                if (bodyTypeMetadataObject is IWebHookBodyTypeMetadataService bodyTypeMetadata)
                {
                    filter = new WebHookEventNameMapperFilter(
                        _requestReader,
                        _loggerFactory,
                        bodyTypeMetadata,
                        (IWebHookEventFromBodyMetadata)eventFromBodyMetadataObject);
                }
                else
                {
                    filter = new WebHookEventNameMapperFilter(_requestReader, _loggerFactory, _metadataProvider);
                }

                filters.Add(filter);
            }
        }

        private void AddGetHeadRequestFilter(IDictionary<object, object> properties, IList<IFilterMetadata> filters)
        {
            if (properties.TryGetValue(typeof(IWebHookGetHeadRequestMetadata), out var getHeadRequestMetadataObject))
            {
                WebHookGetHeadRequestFilter filter;
                if (getHeadRequestMetadataObject is IWebHookGetHeadRequestMetadata getHeadRequestMetadata)
                {
                    filter = new WebHookGetHeadRequestFilter(
                        _configuration,
                        _hostingEnvironment,
                        _loggerFactory,
                        getHeadRequestMetadata);
                }
                else
                {
                    filter = new WebHookGetHeadRequestFilter(
                        _configuration,
                        _hostingEnvironment,
                        _loggerFactory,
                        _metadataProvider);
                }

                filters.Add(filter);
            }
        }

        private void AddPingRequestFilter(IDictionary<object, object> properties, IList<IFilterMetadata> filters)
        {
            if (properties.TryGetValue(typeof(IWebHookPingRequestMetadata), out var pingRequestMetadataObject))
            {
                WebHookPingRequestFilter filter;
                if (pingRequestMetadataObject is IWebHookPingRequestMetadata pingRequestMetadata)
                {
                    filter = new WebHookPingRequestFilter(_loggerFactory, pingRequestMetadata);
                }
                else
                {
                    filter = new WebHookPingRequestFilter(_loggerFactory, _metadataProvider);
                }

                filters.Add(filter);
            }
        }

        private void AddVerifyBodyTypeFilter(IDictionary<object, object> properties, IList<IFilterMetadata> filters)
        {
            WebHookVerifyBodyTypeFilter filter;
            var bodyTypeMetadataObject = properties[typeof(IWebHookBodyTypeMetadataService)];
            if (bodyTypeMetadataObject is IWebHookBodyTypeMetadataService bodyTypeMetadata)
            {
                filter = new WebHookVerifyBodyTypeFilter(_loggerFactory, bodyTypeMetadata);
            }
            else
            {
                filter = new WebHookVerifyBodyTypeFilter(_loggerFactory, _metadataProvider);
            }

            filters.Add(filter);
        }

        private void AddVerifyCodeFilter(IDictionary<object, object> properties, IList<IFilterMetadata> filters)
        {
            if (properties.TryGetValue(typeof(IWebHookVerifyCodeMetadata), out var verifyCodeMetadataObject))
            {
                WebHookVerifyCodeFilter filter;
                if (verifyCodeMetadataObject is IWebHookVerifyCodeMetadata verifyCodeMetadata)
                {
                    filter = new WebHookVerifyCodeFilter(
                        _configuration,
                        _hostingEnvironment,
                        _loggerFactory,
                        verifyCodeMetadata);
                }
                else
                {
                    filter = new WebHookVerifyCodeFilter(
                        _configuration,
                        _hostingEnvironment,
                        _loggerFactory,
                        _metadataProvider);
                }

                filters.Add(filter);
            }
        }

        private void AddVerifyRequiredValueFilter(
            IDictionary<object, object> properties,
            IList<IFilterMetadata> filters)
        {
            if (properties.TryGetValue(typeof(IWebHookBindingMetadata), out var bindingMetadataObject))
            {
                WebHookVerifyRequiredValueFilter filter;
                if (bindingMetadataObject is IWebHookBindingMetadata bindingMetadata)
                {
                    filter = new WebHookVerifyRequiredValueFilter(_loggerFactory, bindingMetadata);
                }
                else
                {
                    filter = new WebHookVerifyRequiredValueFilter(_loggerFactory, _metadataProvider);
                }

                filters.Add(filter);
            }
        }
    }
}
