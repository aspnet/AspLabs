// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes one or more event notifications received as a Pusher WebHook.
    /// For details about Pusher WebHooks, see <c>https://pusher.com/docs/webhooks</c>.
    /// </summary>
    public class PusherNotifications
    {
        private readonly List<IDictionary<string, object>> _events = new List<IDictionary<string, object>>();

        /// <summary>
        ///  Gets or sets a Unix time stamp in milliseconds which can be used to determine the order in which
        ///  Pusher events were generated. For conversion of Unix timestamps to <see cref="DateTimeOffset"/>,
        ///  see <c>https://msdn.microsoft.com/en-us/library/system.datetimeoffset.fromunixtimemilliseconds</c>.
        /// </summary>
        [JsonProperty("time_ms")]
        public long CreatedAt { get; set; }

        /// <summary>
        /// Gets the set of events contained in this notification from a Pusher WebHook. Each notification
        /// is represented as a <see cref="Dictionary{TKey, TValue}"/> where <c>TKey</c> is a property
        /// name and <c>TValue</c> is the value of that property. For each notification, the Action
        /// can be found using the key '<c>name</c>'.
        /// </summary>
        [JsonProperty("events")]
        public ICollection<IDictionary<string, object>> Events
        {
            get
            {
                return _events;
            }
        }
    }
}
