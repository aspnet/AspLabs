// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains information about the location where the media uploaded to Instagram was recorded.
    /// </summary>
    public class InstagramLocation
    {
        /// <summary>
        /// Gets or sets the ID of the location.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the location.
        /// </summary>
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the location.
        /// </summary>
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
}
