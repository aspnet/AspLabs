// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes the revision
    /// </summary>
    public class WorkItemUpdatedRevision
    {
        /// <summary>
        /// Gets the identifier of the revision.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets the revision number.
        /// </summary>
        [JsonProperty("rev")]
        public int Rev { get; set; }

        /// <summary>
        /// Gets the revision fields.
        /// </summary>
        [JsonProperty("fields")]
        public WorkItemFields Fields { get; set; }

        /// <summary>
        /// Gets the revision URL.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}
