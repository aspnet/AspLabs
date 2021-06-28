// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes build drop
    /// </summary>
    public class BuildCompletedDrop
    {
        /// <summary>
        /// Gets drop location.
        /// </summary>
        [JsonProperty("location")]
        public string Location { get; set; }

        /// <summary>
        /// Gets drop type.
        /// </summary>
        [JsonProperty("type")]
        public string DropType { get; set; }

        /// <summary>
        /// Gets drop location URL.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }

        /// <summary>
        /// Gets drop location download URL.
        /// </summary>
        [JsonProperty("downloadUrl")]
        public Uri DownloadUrl { get; set; }
    }
}
