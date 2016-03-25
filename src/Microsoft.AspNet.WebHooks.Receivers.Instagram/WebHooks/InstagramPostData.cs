// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.AspNet.WebHooks.Serialization;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains the data entry for media posted to Instagram
    /// </summary>
    public class InstagramPostData
    {
        private Collection<string> _tags = new Collection<string>();

        /// <summary>
        /// Gets or sets a unique ID for this post.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a direct link to the media in this post.
        /// </summary>
        [JsonProperty("link")]
        public Uri Link { get; set; }

        /// <summary>
        /// Gets or sets the data and time when this post was created.
        /// </summary>
        [JsonProperty("created_time")]
        [JsonConverter(typeof(InstagramUnixTimeConverter))]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets information about the type of media included in this post.
        /// </summary>
        [JsonProperty("type")]
        public string MediaType { get; set; }

        /// <summary>
        /// Gets the tags included in this post.
        /// </summary>
        [JsonProperty("tags")]
        public Collection<string> Tags
        {
            get
            {
                return _tags;
            }
        }

        /// <summary>
        /// Gets or sets the location of the poster.
        /// </summary>
        [JsonProperty("location")]
        public InstagramLocation Location { get; set; }

        /// <summary>
        /// Gets or sets information about any images included in this post.
        /// </summary>
        [JsonProperty("images")]
        public InstagramImages Images { get; set; }

        /// <summary>
        /// Gets or sets information about any videos included in this post.
        /// </summary>
        [JsonProperty("videos")]
        public InstagramVideos Videos { get; set; }

        /// <summary>
        /// Gets or sets information about the caption included in this post.
        /// </summary>
        [JsonProperty("caption")]
        public InstagramCaption Caption { get; set; }

        /// <summary>
        /// Gets or sets information about the user posting the media.
        /// </summary>
        [JsonProperty("user")]
        public InstagramUser User { get; set; }
    }
}
