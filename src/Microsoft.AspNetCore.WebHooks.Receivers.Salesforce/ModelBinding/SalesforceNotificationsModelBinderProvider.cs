// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.AspNetCore.WebHooks.ModelBinding
{
    /// <summary>
    /// An <see cref="IModelBinderProvider"/> for <see cref="SalesforceNotifications"/> instances.
    /// </summary>
    public class SalesforceNotificationsModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(SalesforceNotifications))
            {
                var xElementMetadata = context.Metadata.GetMetadataForType(typeof(XElement));
                var xElementBinder = context.CreateBinder(xElementMetadata);
                return new SalesforceNotificationsModelBinder(xElementBinder, xElementMetadata);
            }

            return null;
        }
    }
}
