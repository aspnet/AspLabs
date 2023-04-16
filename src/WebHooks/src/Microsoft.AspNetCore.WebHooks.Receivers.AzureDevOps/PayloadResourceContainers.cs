// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes containers containing the resource
    /// </summary>
    public class PayloadResourceContainers
    {
        /// <summary>
        /// Gets the collection.
        /// </summary>
        [JsonProperty("collection")]
        public PayloadResourceContainer Collection { get; set; }

        /// <summary>
        /// Gets the account.
        /// </summary>
        [JsonProperty("account")]
        public PayloadResourceContainer Account { get; set; }

        /// <summary>
        /// Gets the project.
        /// </summary>
        [JsonProperty("project")]
        public PayloadResourceContainer Project { get; set; }
    }
}
