// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes the WorkItem's link.
    /// </summary>
    public class WorkItemLink
    {
        /// <summary>
        /// Gets the URL of WorkItem's link.
        /// </summary>
        [JsonProperty("href")]
        public string Href { get; set; }
    }
}
