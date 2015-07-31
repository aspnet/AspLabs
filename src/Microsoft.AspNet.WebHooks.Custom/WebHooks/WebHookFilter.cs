// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Defines a filter which can be applied when registering a WebHook. 
    /// The filter determines which event notifications will get dispatched to a given WebHook. 
    /// That is, depending on which filters a WebHook is created with, it will only see event 
    /// notifications that match one or more of those filters.
    /// </summary>
    public class WebHookFilter
    {
        /// <summary>
        /// Gets or sets the name of the filter, e.g. <c>Blob Update</c>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the filter.
        /// </summary>
        public string Description { get; set; }
    }
}
