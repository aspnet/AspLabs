// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Base class for resource object which describes WorkItem event types.
    /// </summary>
    /// <typeparam name="T">Type which describes fields associated with this kind of WorkItem change</typeparam>
    public abstract class BaseWorkItemResource<T> : BaseResource
    {
        /// <summary>
        /// Gets the identifier of WorkItem.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets the revision number.
        /// </summary>
        [JsonProperty("rev")]
        public int RevisionNumber { get; set; }

        /// <summary>
        /// Gets fields associated with the WorkItem.
        /// </summary>
        [JsonProperty("fields")]
        public T Fields { get; set; }

        /// <summary>
        /// Gets links associated with the WorkItem.
        /// </summary>
        [JsonProperty("_links")]
        public WorkItemLinks Links { get; set; }

        /// <summary>
        /// Gets the URL of the WorkItem.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }    
}
