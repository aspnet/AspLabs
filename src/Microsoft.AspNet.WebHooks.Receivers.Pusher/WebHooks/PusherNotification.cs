// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes one or more event notifications received as a Pusher WebHook.
    /// For details about Pusher WebHooks, see <c>https://pusher.com/docs/webhooks</c>.
    /// </summary>
    public class PusherNotification
    {
        internal const string CreatedAtKey = "time_ms";
        internal const string EventsKey = "events";
        internal const string EventNameKey = "name";

        private readonly IDictionary<string, Collection<JObject>> _events = new Dictionary<string, Collection<JObject>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PusherNotification"/> class. 
        /// </summary>
        public PusherNotification()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PusherNotification"/> class using the given
        /// <paramref name="notification"/>.
        /// </summary>
        /// <param name="notification">A <see cref="JObject"/> containing the events.</param>
        public PusherNotification(JObject notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException("notification");
            }

            CreatedAt = notification.Value<long>(CreatedAtKey);

            JArray events = notification.Value<JArray>(EventsKey);
            if (events != null)
            {
                foreach (JObject e in events)
                {
                    string action = e.Value<string>(EventNameKey);
                    if (action != null)
                    {
                        Collection<JObject> items;
                        if (!Events.TryGetValue(action, out items))
                        {
                            items = new Collection<JObject>();
                            Events[action] = items;
                        }
                        items.Add(e);
                    }
                }
            }
        }

        /// <summary>
        ///  Gets or sets a Unix time stamp in milliseconds which can be used to determine the order in which 
        ///  Pusher events were generated. If desired, the time stamp can be converted using 
        ///  <see cref="M:DateTimeOffset.FromUnixTimeMilliseconds"/>.
        /// </summary>
        public long CreatedAt { get; set; }

        /// <summary>
        /// Gets the set of events contained in this notification from a Pusher WebHook.
        /// </summary>
        public IDictionary<string, Collection<JObject>> Events
        {
            get { return _events; }
        }
    }
}
