// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks.Config
{
    /// <summary>
    /// Contains WebHook settings that are provided by the outside, for example through application settings.
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly", Justification = "This class is not intended for serialization")]
    [Serializable]
    public class SettingsDictionary : Dictionary<string, string>
    {
        private readonly Dictionary<string, ConnectionSettings> _connections = new Dictionary<string, ConnectionSettings>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsDictionary"/> class.
        /// </summary>
        public SettingsDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsDictionary"/> class with the specified serialization information and streaming context.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> containing information about the <see cref="SettingsDictionary"/> to be initialized.</param>
        /// <param name="context">A <see cref="StreamingContext"/> that indicates the source destination and context information of a serialized stream.</param>
        protected SettingsDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the set of connection strings.
        /// </summary>
        public IDictionary<string, ConnectionSettings> Connections
        {
            get
            {
                return _connections;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="KeyNotFoundException"/>, and a set operation creates a new element with the specified key.</returns>
        public new string this[string key]
        {
            get
            {
                try
                {
                    return base[key];
                }
                catch (KeyNotFoundException)
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, CommonResources.Settings_KeyNotFound, key);
                    throw new KeyNotFoundException(msg);
                }
            }

            set
            {
                base[key] = value;
            }
        }

        /// <summary>
        /// Gets the value with the given key, or null if the key is not present. 
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>The value with the specified key if found; otherwise null.</returns>
        public string GetValueOrDefault(string key)
        {
            string value;
            return TryGetValue(key, out value) ? value : null;
        }

        /// <summary>
        /// Sets the entry with the given key to the given value. If value is the default value
        /// then the entry is removed.
        /// </summary>
        /// <param name="key">The key of the entry.</param>
        /// <param name="value">The value (or default value).</param>
        public void SetOrClearValue(string key, string value)
        {
            if (value == null)
            {
                Remove(key);
            }
            else
            {
                this[key] = value;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the key has a value set to 'true'; otherwise <c>false</c>.
        /// </summary>
        /// <param name="key">The key to evaluate the value for.</param>
        /// <returns><c>true</c> if the value is set to 'true'; otherwise <c>false</c>.</returns>
        public bool IsTrue(string key)
        {
            string value = GetValueOrDefault(key);
            if (value != null)
            {
                bool isSet;
                return bool.TryParse(value.Trim(), out isSet) ? isSet : false;
            }
            return false;
        }
    }
}
