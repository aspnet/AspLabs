// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes payload message.
    /// </summary>
    public class PayloadMessage
    {
        /// <summary>
        /// Gets the message in plain text.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets the message in HTML format.
        /// </summary>
        [JsonProperty("html")]
        public string Html { get; set; }

        /// <summary>
        /// Gets the message in markdown format.
        /// </summary>
        [JsonProperty("markdown")]
        public string Markdown { get; set; }
    }
}
