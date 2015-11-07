// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains a Zendesk WebHook payload. For more details about Zendesk WebHooks, 
    /// please see <c>https://developer.zendesk.com/embeddables/docs/ios/push_notifications_webhook</c>.
    /// </summary>
    public class ZendeskPost
    {
        private readonly Collection<ZendeskDevice> _devices = new Collection<ZendeskDevice>();

        /// <summary>
        /// Gets the list of devices for this notification.
        /// </summary>
        [JsonProperty("devices")]
        public Collection<ZendeskDevice> Devices
        {
            get { return _devices; }
        }

        /// <summary>
        /// Gets or sets the actual notification of this Zendesk WebHook payload.
        /// </summary>
        [JsonProperty("notification")]
        public ZendeskNotification Notification { get; set; }
    }
}
