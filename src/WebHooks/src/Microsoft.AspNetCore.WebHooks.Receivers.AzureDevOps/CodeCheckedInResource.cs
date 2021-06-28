// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes the resource that associated with <see cref="CodeCheckedInPayload"/>
    /// </summary>
    public class CodeCheckedInResource : BaseResource
    {
        /// <summary>
        /// Gets the changeset identifier.
        /// </summary>
        [JsonProperty("changesetId")]
        public int ChangesetId { get; set; }

        /// <summary>
        /// Gets the changeset URL.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }

        /// <summary>
        /// Gets the changeset author.
        /// </summary>
        [JsonProperty("author")]
        public ResourceUser Author { get; set; }

        /// <summary>
        /// Gets the user that checked in the changeset.
        /// </summary>
        [JsonProperty("checkedInBy")]
        public ResourceUser CheckedInBy { get; set; }

        /// <summary>
        /// Gets the changeset creation date.
        /// </summary>
        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Gets the changeset comment.
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}
