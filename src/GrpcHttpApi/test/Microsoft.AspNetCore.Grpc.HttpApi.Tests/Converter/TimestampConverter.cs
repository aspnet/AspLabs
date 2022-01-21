// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    public sealed class TimestampConverter : WellKnownTypeConverter
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // Constants determined programmatically, but then hard-coded so they can be constant expressions.
        private const long BclSecondsAtUnixEpoch = 62135596800;
        internal const long UnixSecondsAtBclMaxValue = 253402300799;
        internal const long UnixSecondsAtBclMinValue = -BclSecondsAtUnixEpoch;
        internal const int MaxNanos = Duration.NanosecondsPerSecond - 1;

        private static bool IsNormalized(long seconds, int nanoseconds) =>
            nanoseconds >= 0 &&
            nanoseconds <= MaxNanos &&
            seconds >= UnixSecondsAtBclMinValue &&
            seconds <= UnixSecondsAtBclMaxValue;

        protected override string WellKnownTypeName => Timestamp.Descriptor.FullName;

        public override IMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IMessage value, JsonSerializerOptions options)
        {
            int nanos = (int)value.Descriptor.Fields[Timestamp.NanosFieldNumber].Accessor.GetValue(value);
            long seconds = (long)value.Descriptor.Fields[Timestamp.SecondsFieldNumber].Accessor.GetValue(value);

            if (IsNormalized(seconds, nanos))
            {
                // Use .NET's formatting for the value down to the second, including an opening double quote (as it's a string value)
                DateTime dateTime = UnixEpoch.AddSeconds(seconds);
                var builder = new StringBuilder();
                builder.Append(dateTime.ToString("yyyy'-'MM'-'dd'T'HH:mm:ss", CultureInfo.InvariantCulture));

                if (nanos != 0)
                {
                    builder.Append('.');
                    // Output to 3, 6 or 9 digits.
                    if (nanos % 1000000 == 0)
                    {
                        builder.Append((nanos / 1000000).ToString("d3", CultureInfo.InvariantCulture));
                    }
                    else if (nanos % 1000 == 0)
                    {
                        builder.Append((nanos / 1000).ToString("d6", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(nanos.ToString("d9", CultureInfo.InvariantCulture));
                    }
                }

                builder.Append("Z");

                writer.WriteStringValue(builder.ToString());
            }
            else
            {
                throw new InvalidOperationException("Non-normalized timestamp value");
            }

        }
    }
}
