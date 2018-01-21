// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.WebHooks.Internal
{
    /// <summary>
    /// The default <see cref="IWebHookRequestReader"/> implementation.
    /// </summary>
    public class WebHookRequestReader : IWebHookRequestReader
    {
        private readonly IModelBinder _bodyModelBinder;
        private readonly IModelMetadataProvider _metadataProvider;

        /// <summary>
        /// Instantiates a new <see cref="WebHookRequestReader"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="metadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="optionsAccessor">
        /// The <see cref="IOptions{MvcOptions}"/> accessor for <see cref="MvcOptions"/>.
        /// </param>
        /// <param name="readerFactory">The <see cref="IHttpRequestStreamReaderFactory"/>.</param>
        public WebHookRequestReader(
            ILoggerFactory loggerFactory,
            IModelMetadataProvider metadataProvider,
            IOptions<MvcOptions> optionsAccessor,
            IHttpRequestStreamReaderFactory readerFactory)
        {
            // Do not store options.ValueProviderFactories because that is only the initial value of (for example)
            // ResourceExecutingContext.ValueProviderFactories.
            var options = optionsAccessor.Value;
            _bodyModelBinder = new BodyModelBinder(options.InputFormatters, readerFactory, loggerFactory, options);
            _metadataProvider = metadataProvider;
        }

        /// <inheritdoc />
        public async Task<IFormCollection> ReadAsFormDataAsync(ActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var request = actionContext.HttpContext.Request;
            if (!IsValidPost(request) ||
                !request.HasFormContentType)
            {
                // Filters e.g. WebHookVerifyBodyTypeFilter will log and return errors about these conditions.
                return null;
            }

            // ReadFormAsync does not always ensure the body can be read multiple times.
            await WebHookHttpRequestUtilities.PrepareRequestBody(request);

            // Read request body.
            IFormCollection formCollection;
            try
            {
                formCollection = await request.ReadFormAsync();
            }
            finally
            {
                request.Body.Seek(0L, SeekOrigin.Begin);
            }

            return formCollection;
        }

        /// <inheritdoc />
        /// <remarks>This method assumes the necessary input formatters have been registered.</remarks>
        public async Task<TModel> ReadBodyAsync<TModel>(ActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var request = actionContext.HttpContext.Request;
            if (!IsValidPost(request))
            {
                // Filters e.g. WebHookVerifyBodyTypeFilter will log and return errors about these conditions.
                return default;
            }

            var modelMetadata = _metadataProvider.GetMetadataForType(typeof(TModel));
            var bindingContext = DefaultModelBindingContext.CreateBindingContext(
                actionContext,
                new CompositeValueProvider(),
                modelMetadata,
                bindingInfo: null,
                modelName: WebHookConstants.ModelStateBodyModelName);

            // Read request body.
            try
            {
                await _bodyModelBinder.BindModelAsync(bindingContext);
            }
            finally
            {
                request.Body.Seek(0L, SeekOrigin.Begin);
            }

            if (!bindingContext.ModelState.IsValid)
            {
                return default;
            }

            if (!bindingContext.Result.IsModelSet)
            {
                throw new InvalidOperationException(Resources.RequestReader_ModelBindingFailed);
            }

            // Success
            return (TModel)bindingContext.Result.Model;
        }

        private bool IsValidPost(HttpRequest request)
        {
            return request.Body != null &&
                request.ContentLength.HasValue &&
                request.ContentLength.Value > 0L &&
                HttpMethods.IsPost(request.Method);
        }
    }
}
