// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes container
    /// </summary>
    public class PayloadResourceContainer
    {
        /// <summary>
        /// Gets the identifier of container.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
