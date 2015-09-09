// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information about a media caption from Instagram.
    /// </summary>
    public class InstagramCaption
    {
        /// <summary>
        /// Gets or sets the ID of this caption.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the creation time for this caption.
        /// </summary>
        [JsonProperty("created_time")]
        public string CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the text of this caption.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="InstagramUser"/> that this caption is from.
        /// </summary>
        [JsonProperty("from")]
        public InstagramUser From { get; set; }
    }
}
