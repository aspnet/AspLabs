// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    internal sealed class TimestampConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        private static readonly Regex TimestampRegex = new Regex(@"^(?<datetime>[0-9]{4}-[01][0-9]-[0-3][0-9]T[012][0-9]:[0-5][0-9]:[0-5][0-9])(?<subseconds>\.[0-9]{1,9})?(?<offset>(Z|[+-][0-1][0-9]:[0-5][0-9]))$", RegexOptions.Compiled);
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // Constants determined programmatically, but then hard-coded so they can be constant expressions.
        private const long BclSecondsAtUnixEpoch = 62135596800;
        internal const long UnixSecondsAtBclMaxValue = 253402300799;
        internal const long UnixSecondsAtBclMinValue = -BclSecondsAtUnixEpoch;
        internal const int MaxNanos = Duration.NanosecondsPerSecond - 1;
        private static readonly int[] SubsecondScalingFactors = { 0, 100000000, 100000000, 10000000, 1000000, 100000, 10000, 1000, 100, 10, 1 };

        public TimestampConverter(JsonSettings settings)
        {
        }

        private static bool IsNormalized(long seconds, int nanoseconds) =>
            nanoseconds >= 0 &&
            nanoseconds <= MaxNanos &&
            seconds >= UnixSecondsAtBclMinValue &&
            seconds <= UnixSecondsAtBclMaxValue;

        public override TMessage? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new InvalidOperationException("Expected string value for Timestamp");
            }
            var match = TimestampRegex.Match(reader.GetString()!);
            if (!match.Success)
            {
                throw new InvalidOperationException($"Invalid Timestamp value: {reader.GetString()}");
            }
            var dateTime = match.Groups["datetime"].Value;
            var subseconds = match.Groups["subseconds"].Value;
            var offset = match.Groups["offset"].Value;

            try
            {
                DateTime parsed = DateTime.ParseExact(
                    dateTime,
                    "yyyy-MM-dd'T'HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                // TODO: It would be nice not to have to create all these objects... easy to optimize later though.
                Timestamp timestamp = Timestamp.FromDateTime(parsed);
                int nanosToAdd = 0;
                if (subseconds != "")
                {
                    // This should always work, as we've got 1-9 digits.
                    int parsedFraction = int.Parse(subseconds.Substring(1), CultureInfo.InvariantCulture);
                    nanosToAdd = parsedFraction * SubsecondScalingFactors[subseconds.Length];
                }
                int secondsToAdd = 0;
                if (offset != "Z")
                {
                    // This is the amount we need to *subtract* from the local time to get to UTC - hence - => +1 and vice versa.
                    int sign = offset[0] == '-' ? 1 : -1;
                    int hours = int.Parse(offset.Substring(1, 2), CultureInfo.InvariantCulture);
                    int minutes = int.Parse(offset.Substring(4, 2));
                    int totalMinutes = hours * 60 + minutes;
                    if (totalMinutes > 18 * 60)
                    {
                        throw new InvalidOperationException($"Invalid Timestamp value: {reader.GetString()}");
                    }
                    if (totalMinutes == 0 && sign == 1)
                    {
                        // This is an offset of -00:00, which means "unknown local offset". It makes no sense for a timestamp.
                        throw new InvalidOperationException($"Invalid Timestamp value: {reader.GetString()}");
                    }
                    // We need to *subtract* the offset from local time to get UTC.
                    secondsToAdd = sign * totalMinutes * 60;
                }
                // Ensure we've got the right signs. Currently unnecessary, but easy to do.
                if (secondsToAdd < 0 && nanosToAdd > 0)
                {
                    secondsToAdd++;
                    nanosToAdd = nanosToAdd - Duration.NanosecondsPerSecond;
                }
                if (secondsToAdd != 0 || nanosToAdd != 0)
                {
                    timestamp += new Duration { Nanos = nanosToAdd, Seconds = secondsToAdd };
                    // The resulting timestamp after offset change would be out of our expected range. Currently the Timestamp message doesn't validate this
                    // anywhere, but we shouldn't parse it.
                    if (timestamp.Seconds < UnixSecondsAtBclMinValue || timestamp.Seconds > UnixSecondsAtBclMaxValue)
                    {
                        throw new InvalidOperationException($"Invalid Timestamp value: {reader.GetString()}");
                    }
                }

                var message = new TMessage();
                message.Descriptor.Fields[Timestamp.SecondsFieldNumber].Accessor.SetValue(message, timestamp.Seconds);
                message.Descriptor.Fields[Timestamp.NanosFieldNumber].Accessor.SetValue(message, timestamp.Nanos);
                return message;
            }
            catch (FormatException)
            {
                throw new InvalidOperationException($"Invalid Timestamp value: {reader.GetString()}");
            }
        }

        public override void Write(Utf8JsonWriter writer, TMessage value, JsonSerializerOptions options)
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
