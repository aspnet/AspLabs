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
    internal sealed class DurationConverter<TMessage> : JsonConverter<TMessage> where TMessage : IMessage, new()
    {
        private static readonly Regex DurationRegex = new Regex(@"^(?<sign>-)?(?<int>[0-9]{1,12})(?<subseconds>\.[0-9]{1,9})?s$", RegexOptions.Compiled);

        //private static readonly Regex TimestampRegex = new Regex(@"^(?<datetime>[0-9]{4}-[01][0-9]-[0-3][0-9]T[012][0-9]:[0-5][0-9]:[0-5][0-9])(?<subseconds>\.[0-9]{1,9})?(?<offset>(Z|[+-][0-1][0-9]:[0-5][0-9]))$", RegexOptions.Compiled);
        //private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //// Constants determined programmatically, but then hard-coded so they can be constant expressions.
        private const long BclSecondsAtUnixEpoch = 62135596800;
        internal const long UnixSecondsAtBclMaxValue = 253402300799;
        internal const long UnixSecondsAtBclMinValue = -BclSecondsAtUnixEpoch;
        internal const int MaxNanos = Duration.NanosecondsPerSecond - 1;
        private static readonly int[] SubsecondScalingFactors = { 0, 100000000, 100000000, 10000000, 1000000, 100000, 10000, 1000, 100, 10, 1 };

        public DurationConverter(JsonSettings settings)
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
                throw new InvalidOperationException("Expected string value for Duration");
            }
            var match = DurationRegex.Match(reader.GetString()!);
            if (!match.Success)
            {
                throw new InvalidOperationException("Invalid Duration value: " + reader.GetString());
            }
            var sign = match.Groups["sign"].Value;
            var secondsText = match.Groups["int"].Value;
            // Prohibit leading insignficant zeroes
            if (secondsText[0] == '0' && secondsText.Length > 1)
            {
                throw new InvalidOperationException("Invalid Duration value: " + reader.GetString());
            }
            var subseconds = match.Groups["subseconds"].Value;
            var multiplier = sign == "-" ? -1 : 1;

            try
            {
                long seconds = long.Parse(secondsText, CultureInfo.InvariantCulture) * multiplier;
                int nanos = 0;
                if (subseconds != "")
                {
                    // This should always work, as we've got 1-9 digits.
                    int parsedFraction = int.Parse(subseconds.Substring(1));
                    nanos = parsedFraction * SubsecondScalingFactors[subseconds.Length] * multiplier;
                }
                if (!IsNormalized(seconds, nanos))
                {
                    throw new InvalidOperationException("Invalid Duration value: " + reader.GetString());
                }
                var message = new TMessage();
                message.Descriptor.Fields[Duration.SecondsFieldNumber].Accessor.SetValue(message, seconds);
                message.Descriptor.Fields[Duration.NanosFieldNumber].Accessor.SetValue(message, nanos);
                return message;
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid Duration value: " + reader.GetString());
            }
        }

        public override void Write(Utf8JsonWriter writer, TMessage value, JsonSerializerOptions options)
        {
            int nanos = (int)value.Descriptor.Fields[Duration.NanosFieldNumber].Accessor.GetValue(value);
            long seconds = (long)value.Descriptor.Fields[Duration.SecondsFieldNumber].Accessor.GetValue(value);

            if (IsNormalized(seconds, nanos))
            {
                var builder = new StringBuilder();
                // The seconds part will normally provide the minus sign if we need it, but not if it's 0...
                if (seconds == 0 && nanos < 0)
                {
                    builder.Append('-');
                }

                builder.Append(seconds.ToString("d", CultureInfo.InvariantCulture));
                AppendNanoseconds(builder, Math.Abs(nanos));
                builder.Append("s");
                writer.WriteStringValue(builder.ToString());
            }
            else
            {
                throw new InvalidOperationException("Non-normalized duration value");
            }
        }

        /// <summary>
        /// Appends a number of nanoseconds to a StringBuilder. Either 0 digits are added (in which
        /// case no "." is appended), or 3 6 or 9 digits. This is internal for use in Timestamp as well
        /// as Duration.
        /// </summary>
        internal static void AppendNanoseconds(StringBuilder builder, int nanos)
        {
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
        }
    }
}
