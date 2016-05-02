// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Describes user entity
    /// </summary>
    public class ResourceUser
    {
        /// <summary>
        /// Gets the identifier of the user.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets the user display name.
        /// </summary>
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the user unique name.
        /// </summary>
        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets the user URL.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }

        /// <summary>
        /// Gets the user image URL.
        /// </summary>
        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }
    }
}
