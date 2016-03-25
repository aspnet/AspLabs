// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes the data portion of an Instagram WebHook event notification. For details about Instagram WebHooks, please 
    /// see <c>https://www.instagram.com/developer/subscriptions/</c>.
    /// </summary>
    public class InstagramNotificationData
    {
        /// <summary>
        /// Gets or sets the ID of the media that was added.
        /// </summary>
        [JsonProperty("media_id")]
        public string MediaId { get; set; }
    }
}
