// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains the information about notifications
    /// </summary>
    public class ZendeskNotification
    {
        /// <summary>
        /// The short message of the notification
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// The long message of the notification
        /// </summary>
        [JsonProperty("body")]
        public string Body { get; set; }

        /// <summary>
        /// The identifier of the ticket that was updated. Pass this along as <c>zendesk_sdk_request_id</c> if you
        /// want ticket deep-linking in the application.
        /// </summary>
        [JsonProperty("ticket_id")]
        public string TicketId { get; set; }
    }
}
