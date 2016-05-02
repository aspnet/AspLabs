// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks.Payloads
{
    /// <summary>
    /// Describes the resource that associated with <see cref="TeamRoomMessagePostedPayload"/>
    /// </summary>
    public class TeamRoomMessagePostedResource : BaseResource
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets the content of the message.
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        [JsonProperty("messageType")]
        public string MessageType { get; set; }

        /// <summary>
        /// Gets the posted time of the message.
        /// </summary>
        [JsonProperty("postedTime")]
        public DateTime PostedTime { get; set; }

        /// <summary>
        /// Gets the room identifier where message was posted.
        /// </summary>
        [JsonProperty("postedRoomId")]
        public int PostedRoomId { get; set; }

        /// <summary>
        /// Gets the user who posted the message.
        /// </summary>
        [JsonProperty("postedBy")]
        public ResourceUser PostedBy { get; set; }
    }
}
