// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// A container for the data associated with the notification.
    /// </summary>
    public class IntercomNotificationData
    {

        /// <summary>
        /// Gets or sets the data associated with the notification, which will have a 'type' field.
        /// </summary>
        [JsonProperty("item", Required = Required.Always)]
        public object Item { get; set; }
    }
}
