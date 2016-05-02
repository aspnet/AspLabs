// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Describes the resource that associated with <see cref="WorkItemUpdatedPayload"/>
    /// </summary>
    public class WorkItemUpdatedResource : BaseWorkItemResource<WorkItemUpdatedFields>
    {
        /// <summary>
        /// Gets WorkItem identifier.
        /// </summary>
        [JsonProperty("workItemId")]
        public int WorkItemId { get; set; }

        /// <summary>
        /// Gets the author of revision.
        /// </summary>
        [JsonProperty("revisedBy")]
        public ResourceUser RevisedBy { get; set; }

        /// <summary>
        /// Gets the revised date.
        /// </summary>
        [JsonProperty("revisedDate")]
        public DateTime RevisedDate { get; set; }

        /// <summary>
        /// Gets the revision.
        /// </summary>
        [JsonProperty("revision")]
        public WorkItemUpdatedRevision Revision { get; set; }
    }    
}
