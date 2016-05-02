// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Describes build log 
    /// </summary>
    public class BuildCompletedLog
    {
        /// <summary>
        /// Gets the log type.
        /// </summary>
        [JsonProperty("type")]
        public string LogType { get; set; }

        /// <summary>
        /// Gets the log URL.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }

        /// <summary>
        /// Gets the log download URL.
        /// </summary>
        [JsonProperty("downloadUrl")]
        public Uri DownloadUrl { get; set; }
    }
}
