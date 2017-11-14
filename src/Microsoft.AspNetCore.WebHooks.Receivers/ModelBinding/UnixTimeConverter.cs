// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.WebHooks.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.AspNetCore.WebHooks.ModelBinding
{
    /// <summary>
    /// Converts a Unix time stamp to and from a <see cref="DateTime"/>.
    /// </summary>
    public class UnixTimeConverter : DateTimeConverterBase
    {
        private static readonly DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly bool _stringConverter;

        /// <summary>
        /// Converts string or integer values to a <see cref="DateTime"/>. By default the
        /// <see cref="DateTime"/> gets serialized to an integer.
        /// </summary>
        public UnixTimeConverter()
            : this(false)
        {
        }

        /// <summary>
        /// Converts string values to a <see cref="DateTime"/>. By default the
        /// <see cref="DateTime"/> gets serialized to an integer.
        /// </summary>
        /// <param name="stringConverter">
        /// When <see langword="true"/> only deserializes string values and serializes to a string value;
        /// otherwise deserializes string and integer values and serializes to an integer value.
        /// </param>
        protected UnixTimeConverter(bool stringConverter)
        {
            _stringConverter = stringConverter;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var utc = ((DateTime)value).ToUniversalTime();
            var time = (long)(utc - _Epoch).TotalSeconds;
            if (_stringConverter)
            {
                writer.WriteValue(time.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteValue(time);
            }
        }

        /// <inheritdoc />
        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            if (reader.Value == null)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.DateTime_NullError,
                    nameof(DateTime));
                throw new InvalidOperationException(message);
            }

            long time = 0;
            if (reader.TokenType == JsonToken.String || _stringConverter)
            {
                if (!long.TryParse(reader.Value as string, out time))
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.DateTime_BadFormat,
                        reader.Value,
                        nameof(DateTime));
                    throw new InvalidOperationException(message);
                }
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                time = Convert.ToInt64(reader.Value, CultureInfo.InvariantCulture);
            }
            else
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    Resources.DateTime_BadFormat,
                    reader.Value,
                    nameof(DateTime));
                throw new InvalidOperationException(message);
            }

            var utc = _Epoch.AddSeconds(time);
            return utc;
        }
    }
}
