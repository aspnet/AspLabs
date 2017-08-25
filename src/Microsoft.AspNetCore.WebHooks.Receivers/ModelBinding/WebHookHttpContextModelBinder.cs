// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.WebHooks.ModelBinding
{
    /// <summary>
    /// <see cref="IModelBinder"/> implementation to bind models from <see cref="HttpContext.Items"/> entries.
    /// </summary>
    public class WebHookHttpContextModelBinder : IModelBinder
    {
        private readonly IList<Type> _allowedTypes;
        private readonly ILogger _logger;
        private readonly MvcOptions _mvcOptions;

        /// <summary>
        /// Instantiates a new <see cref="WebHookHttpContextModelBinder"/> instance.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="optionsAccessor">The accessor for the <see cref="WebHookOptions"/>.</param>
        /// <param name="mvcOptionsAccessor">The accessor for the <see cref="MvcOptions"/>.</param>
        public WebHookHttpContextModelBinder(
            ILoggerFactory loggerFactory,
            IOptions<WebHookOptions> optionsAccessor,
            IOptions<MvcOptions> mvcOptionsAccessor)
        {
            _allowedTypes = optionsAccessor.Value.HttpContextItemsTypes;
            _logger = loggerFactory.CreateLogger<WebHookHttpContextModelBinder>();
            _mvcOptions = mvcOptionsAccessor.Value;
        }

        /// <inheritdoc />
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // As used here, a key into HttpContext.Items is the exact type of the value stored there.
            Type lookupType = null;
            var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;
            for (var i = 0; i < _allowedTypes.Count; i++)
            {
                var allowedType = _allowedTypes[i];
                if (modelType.IsAssignableFrom(allowedType))
                {
                    lookupType = allowedType;
                    break;
                }
            }

            if (lookupType == null)
            {
                _logger.LogCritical(
                    0,
                    "'{ModelBinderType}' associated with model of unsupported type '{ModelType}'.",
                    typeof(WebHookHttpContextModelBinder),
                    bindingContext.ModelType);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.HttpContextModelBinder_UnsupportedType,
                    typeof(WebHookHttpContextModelBinder),
                    bindingContext.ModelType);
                throw new InvalidOperationException(message);
            }

            // Now know where to look. Do the actual binding.
            if (bindingContext.HttpContext.Items.TryGetValue(lookupType, out var model) && model != null)
            {
                bindingContext.Result = ModelBindingResult.Success(model);
            }
            else if (!_mvcOptions.AllowEmptyInputInBodyModelBinding)
            {
                // Use same ModelStateDictionary key and message as BodyModelBinder would. Note
                // AllowEmptyInputInBodyModelBinding is false by default i.e. failures here are not normally silent.
                string modelBindingKey;
                if (bindingContext.IsTopLevelObject)
                {
                    modelBindingKey = bindingContext.BinderModelName ?? string.Empty;
                }
                else
                {
                    modelBindingKey = bindingContext.ModelName;
                }

                var message = bindingContext
                    .ModelMetadata
                    .ModelBindingMessageProvider
                    .MissingRequestBodyRequiredValueAccessor();

                bindingContext.ModelState.AddModelError(modelBindingKey, message);
            }

            return Task.CompletedTask;
        }
    }
}
