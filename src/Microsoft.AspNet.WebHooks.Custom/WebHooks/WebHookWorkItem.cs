// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// A work item represents the act of firing a single WebHook with one or more notifications.
    /// </summary>
    public class WebHookWorkItem
    {
        private string _id;
        private IEnumerable<NotificationDictionary> _notifications;
        private IDictionary<string, object> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookWorkItem"/> with the given <paramref name="notifications"/>.
        /// </summary>
        public WebHookWorkItem(WebHook webHook, IEnumerable<NotificationDictionary> notifications)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }
            if (notifications == null)
            {
                throw new ArgumentNullException("notifications");
            }

            WebHook = webHook;
            _notifications = notifications;
        }

        /// <summary>
        /// Gets or sets a unique ID which is used to identify this firing of a <see cref="WebHooks.WebHook"/>.
        /// </summary>
        public string Id
        {
            get
            {
                if (_id == null)
                {
                    _id = Guid.NewGuid().ToString("N");
                }
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="WebHooks.WebHook"/> to fire.
        /// </summary>
        public WebHook WebHook { get; set; }

        /// <summary>
        /// Gets or sets the offset (starting with zero) identifying the launch line to be used when firing.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets the set of <see cref="NotificationDictionary"/> that caused the WebHook to be fired.
        /// </summary>
        public IEnumerable<NotificationDictionary> Notifications
        {
            get
            {
                return _notifications;
            }
        }

        /// <summary>
        /// Gets the set of additional properties associated with this <see cref="WebHookWorkItem"/> instance.
        /// </summary>
        public IDictionary<string, object> Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = new Dictionary<string, object>();
                }
                return _properties;
            }
        }
    }
}
