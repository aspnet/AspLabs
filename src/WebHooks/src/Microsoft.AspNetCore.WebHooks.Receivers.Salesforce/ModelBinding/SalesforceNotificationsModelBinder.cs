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
        private readonly IModelBinder _bodyModelBinder;

        /// <summary>
        /// Instantiates a new <see cref="SalesforceNotificationsModelBinder"/> instance.
        /// </summary>
        /// <param name="bodyModelBinder">The <see cref="IModelBinder"/> to bind models from the request body.</param>
        public SalesforceNotificationsModelBinder(IModelBinder bodyModelBinder)
        {
            _bodyModelBinder = bodyModelBinder ?? throw new ArgumentNullException(nameof(bodyModelBinder));
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
            var xElementMetadata = bindingContext.ModelMetadata.GetMetadataForType(typeof(XElement));
            using (var innerContext = bindingContext.EnterNestedScope(
                xElementMetadata,
                bindingContext.FieldName,
                bindingContext.ModelName,
                bindingContext.Model))
            {
                await _bodyModelBinder.BindModelAsync(bindingContext);
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
