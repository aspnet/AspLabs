// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information about the size and address of the media uploaded to Instagram.
    /// </summary>
    public class InstagramMedia
    {
        /// <summary>
        /// Gets or sets the URI of the media.
        /// </summary>
        [JsonProperty("url")]
        public Uri Address { get; set; }

        /// <summary>
        /// Gets or sets the width of the media in pixels.
        /// </summary>
        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the media in pixels.
        /// </summary>
        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
