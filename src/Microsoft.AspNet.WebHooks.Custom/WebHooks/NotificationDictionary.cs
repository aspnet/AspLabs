// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Contains WebHook settings that are provided by the outside, for example through application settings.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "This class is not intended for XML serialization")]
    [Serializable]
    public class NotificationDictionary : Dictionary<string, object>
    {
        internal const string ActionKey = "Action";

        /// <summary>
        /// Initializes a new empty <see cref="NotificationDictionary"/>.
        /// </summary>
        public NotificationDictionary()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationDictionary"/> class.
        /// </summary>
        /// <param name="action">An action describing the notification. In order for the actions to match
        /// the WebHook filter, it must match one or more of the filter values registered with the 
        /// <see cref="IWebHookFilterManager"/>.</param>
        /// <param name="data">Optional additional data to include in the WebHook request.</param>
        public NotificationDictionary(string action, object data)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            this[ActionKey] = action;
            IDictionary<string, object> dataAsDictionary = data as IDictionary<string, object>;
            if (dataAsDictionary == null && data != null)
            {
                dataAsDictionary = new Dictionary<string, object>();
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(data);
                foreach (PropertyDescriptor prop in properties)
                {
                    object val = prop.GetValue(data);
                    dataAsDictionary.Add(prop.Name, val);
                }
            }

            if (dataAsDictionary != null)
            {
                foreach (KeyValuePair<string, object> item in dataAsDictionary)
                {
                    this[item.Key] = item.Value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationDictionary"/> class with the specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> containing information about the <see cref="NotificationDictionary"/> to be initialized.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that indicates the source destination and context information of a serialized stream.</param>
        protected NotificationDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets or sets the Action for this instance.
        /// </summary>
        public string Action
        {
            get
            {
                return this.GetValueOrDefault<string>(ActionKey);
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this[ActionKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="KeyNotFoundException"/>, and a set operation creates a new element with the specified key.</returns>
        public new object this[string key]
        {
            get
            {
                try
                {
                    return base[key];
                }
                catch (KeyNotFoundException)
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, CustomResources.Notification_KeyNotFound, key);
                    throw new KeyNotFoundException(msg);
                }
            }

            set
            {
                base[key] = value;
            }
        }
    }
}
