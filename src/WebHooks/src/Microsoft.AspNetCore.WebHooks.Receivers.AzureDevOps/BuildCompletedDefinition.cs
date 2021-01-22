// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes build definition
    /// </summary>
    public class BuildCompletedDefinition
    {
        /// <summary>
        /// Gets the size of the batch.
        /// </summary>
        [JsonProperty("batchSize")]
        public int BatchSize { get; set; }

        /// <summary>
        /// Gets the trigger type.
        /// </summary>
        [JsonProperty("triggerType")]
        public string TriggerType { get; set; }

        /// <summary>
        /// Gets the trigger type.
        /// </summary>
        [JsonProperty("definitionType")]
        public string DefinitionType { get; set; }

        /// <summary>
        /// Gets the identifier of the build definition.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets the name of the build definition.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the URL of the build definition.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}
