// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains the information about registered device for Zendesk (iOS, Android)
    /// </summary>
    public class ZendeskDevice
    {
        /// <summary>
        /// The device identifier/token that was registered through the SDK
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        /// <summary>
        /// The device type. Possible values: "iOS" or "Android"
        /// </summary>
        [JsonProperty("type")]
        public string DeviceType { get; set; }
    }
}
