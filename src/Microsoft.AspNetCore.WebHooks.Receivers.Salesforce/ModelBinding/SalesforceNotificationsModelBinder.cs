// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.WebHooks.ModelBinding
{
    /// <summary>
    /// An <see cref="IModelBinder"/> for <see cref="SalesforceNotifications"/> instances.
    /// </summary>
    public class SalesforceNotificationsModelBinder : IModelBinder
    {
        private readonly IModelBinder _xElementBinder;
        private readonly ModelMetadata _xElementMetadata;

        /// <summary>
        /// Instantiates a new <see cref="SalesforceNotificationsModelBinder"/> instance.
        /// </summary>
        /// <param name="xElementBinder">An <see cref="IModelBinder"/> for the <see cref="XElement"/> type.</param>
        /// <param name="xElementMetadata">The <see cref="ModelMetadata"/> for the <see cref="XElement"/> type.</param>
        public SalesforceNotificationsModelBinder(IModelBinder xElementBinder, ModelMetadata xElementMetadata)
        {
            if (xElementBinder == null)
            {
                throw new ArgumentNullException(nameof(xElementBinder));
            }
            if (xElementMetadata == null)
            {
                throw new ArgumentNullException(nameof(xElementMetadata));
            }

            _xElementBinder = xElementBinder;
            _xElementMetadata = xElementMetadata;
        }

        /// <inheritdoc />
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // Try to get the XElement.
            ModelBindingResult result;
            using (var innerContext = bindingContext.EnterNestedScope(
                _xElementMetadata,
                bindingContext.FieldName,
                bindingContext.ModelName,
                bindingContext.Model))
            {
                await _xElementBinder.BindModelAsync(bindingContext);
                result = bindingContext.Result;
            }

            // If we got the XElement, create the SalesforceNotifications instance and return that.
            if (result.IsModelSet)
            {
                result = ModelBindingResult.Success(new SalesforceNotifications((XElement)result.Model));
            }

            bindingContext.Result = result;
        }
    }
}
