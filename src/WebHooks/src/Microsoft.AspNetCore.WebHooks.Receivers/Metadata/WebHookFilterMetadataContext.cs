// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// A context for WebHook filter providers i.e. <see cref="IWebHookFilterMetadata"/> implementations.
    /// </summary>
    public class WebHookFilterMetadataContext
    {
        /// <summary>
        /// Initializes a new <see cref="WebHookFilterMetadataContext"/> instance for use from an
        /// <see cref="IApplicationModelProvider"/> implementation.
        /// </summary>
        /// <param name="actionModel">The <see cref="Mvc.ApplicationModels.ActionModel"/> of the action.</param>
        public WebHookFilterMetadataContext(ActionModel actionModel)
        {
            ActionModel = actionModel ?? throw new ArgumentNullException(nameof(actionModel));
        }

        /// <summary>
        /// Initializes a new <see cref="WebHookFilterMetadataContext"/> instance for use within a WebHook request.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="Mvc.Abstractions.ActionDescriptor"/> of the action.</param>
        public WebHookFilterMetadataContext(ActionDescriptor actionDescriptor)
        {
            ActionDescriptor = actionDescriptor ?? throw new ArgumentNullException(nameof(actionDescriptor));
        }

        /// <summary>
        /// Gets the <see cref="Mvc.ApplicationModels.ActionModel"/> of the action if calling
        /// <see cref="IWebHookFilterMetadata.AddFilters"/> from an <see cref="IApplicationModelProvider"/>
        /// implementation.
        /// </summary>
        /// <remarks>
        /// <see cref="IWebHookFilterMetadata"/> implementations should not write
        /// <see cref="Mvc.ApplicationModels.ActionModel"/> properties.
        /// </remarks>
        /// <value>
        /// The <see cref="Mvc.ApplicationModels.ActionModel"/> of the action if calling
        /// <see cref="IWebHookFilterMetadata.AddFilters"/> from an <see cref="IApplicationModelProvider"/>
        /// implementation; <see langword="null"/> otherwise.
        /// </value>
        public ActionModel ActionModel { get; }

        /// <summary>
        /// Gets the <see cref="Mvc.Abstractions.ActionDescriptor"/> of the action if calling
        /// <see cref="IWebHookFilterMetadata.AddFilters"/> within a WebHook request.
        /// </summary>
        /// <remarks>
        /// <see cref="IWebHookFilterMetadata"/> implementations should not write
        /// <see cref="Mvc.Abstractions.ActionDescriptor"/> properties.
        /// </remarks>
        /// <value>
        /// The <see cref="Mvc.Abstractions.ActionDescriptor"/> of the action if calling
        /// <see cref="IWebHookFilterMetadata.AddFilters"/> from within a WebHook request; <see langword="null"/>
        /// otherwise.
        /// </value>
        public ActionDescriptor ActionDescriptor { get; }

        /// <summary>
        /// Gets the collection of receiver-specific WebHook <see cref="IFilterMetadata"/> implementations added so
        /// far. <see cref="IWebHookFilterMetadata"/> implementations should add receiver-specific filters
        /// to this collection.
        /// </summary>
        /// <value>
        /// Initially empty. The collection does not include default filters added based on
        /// <see cref="IWebHookMetadata"/> about a receiver. Nor does it include other filters from
        /// <see cref="ActionModel.Filters"/> or <see cref="ActionDescriptor.FilterDescriptors"/>.
        /// </value>
        public IList<IFilterMetadata> Results { get; } = new List<IFilterMetadata>();
    }
}
