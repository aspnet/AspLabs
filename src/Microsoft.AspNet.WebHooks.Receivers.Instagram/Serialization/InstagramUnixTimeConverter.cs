// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.AspNet.WebHooks.Serialization
{
    /// <summary>
    /// Converts the Instagram string representation of a Unix time stamp to and from a <see cref="DateTime"/>.
    /// </summary>
    internal class InstagramUnixTimeConverter : DateTimeConverterBase
    {
        private static readonly DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            DateTime utc = ((DateTime)value).ToUniversalTime();
            long time = (long)(utc - _Epoch).TotalSeconds;
            writer.WriteValue(time.ToString(CultureInfo.InvariantCulture));
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            if (reader.Value == null)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, InstagramReceiverResources.DateTime_NullError, typeof(DateTime).Name);
                throw new InvalidOperationException(msg);
            }

            long time;
            if (!long.TryParse(reader.Value as string, out time))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, InstagramReceiverResources.DateTime_BadFormat, reader.Value, typeof(DateTime).Name);
                throw new InvalidOperationException(msg);
            }

            DateTime utc = _Epoch.AddSeconds(time);
            return utc;
        }
    }
}
