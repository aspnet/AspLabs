// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes links of the WorkItem.
    /// </summary>
    public class WorkItemLinks
    {
        /// <summary>
        /// Gets the link to the WorkItem itself.
        /// </summary>
        [JsonProperty("self")]
        public WorkItemLink Self { get; set; }

        /// <summary>
        /// Gets the link to the parent WorkItem if exists.
        /// </summary>
        [JsonProperty("parent")]
        public WorkItemLink Parent { get; set; }

        /// <summary>
        /// Gets the link to the WorkItem' updates.
        /// </summary>
        [JsonProperty("workItemUpdates")]
        public WorkItemLink WorkItemUpdates { get; set; }

        /// <summary>
        /// Gets the link to the WorkItem's revisions.
        /// </summary>
        [JsonProperty("workItemRevisions")]
        public WorkItemLink WorkItemRevisions { get; set; }

        /// <summary>
        /// Gets the link to the WorkItem's type.
        /// </summary>
        [JsonProperty("workItemType")]
        public WorkItemLink WorkItemType { get; set; }

        /// <summary>
        /// Gets the link to the WorkItem's fields.
        /// </summary>
        [JsonProperty("fields")]
        public WorkItemLink Fields { get; set; }

        /// <summary>
        /// Gets the link to the WorkItem's HTML.
        /// </summary>
        [JsonProperty("html")]
        public WorkItemLink Html { get; set; }

        /// <summary>
        /// Gets the link to the WorkItem's history.
        /// </summary>
        [JsonProperty("workItemHistory")]
        public WorkItemLink WorkItemHistory { get; set; }
    }
}
