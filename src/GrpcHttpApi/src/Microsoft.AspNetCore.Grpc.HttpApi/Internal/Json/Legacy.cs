#region Copyright notice and license
// Protocol Buffers - Google's data interchange format
// Copyright 2008 Google Inc.  All rights reserved.
// https://developers.google.com/protocol-buffers/
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Internal.Json
{
    // Source here is from https://github.com/protocolbuffers/protobuf
    // Most of this code will be replaced over time with optimized implementations.
    internal static class Legacy
    {
        private static readonly Regex TimestampRegex = new Regex(@"^(?<datetime>[0-9]{4}-[01][0-9]-[0-3][0-9]T[012][0-9]:[0-5][0-9]:[0-5][0-9])(?<subseconds>\.[0-9]{1,9})?(?<offset>(Z|[+-][0-1][0-9]:[0-5][0-9]))$", RegexOptions.Compiled);
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        // Constants determined programmatically, but then hard-coded so they can be constant expressions.
        private const long BclSecondsAtUnixEpoch = 62135596800;
        internal const long UnixSecondsAtBclMaxValue = 253402300799;
        internal const long UnixSecondsAtBclMinValue = -BclSecondsAtUnixEpoch;
        internal const int MaxNanos = Duration.NanosecondsPerSecond - 1;
        private static readonly int[] SubsecondScalingFactors = { 0, 100000000, 100000000, 10000000, 1000000, 100000, 10000, 1000, 100, 10, 1 };

        private static readonly Regex DurationRegex = new Regex(@"^(?<sign>-)?(?<int>[0-9]{1,12})(?<subseconds>\.[0-9]{1,9})?s$", RegexOptions.Compiled);

        public static (long seconds, int nanos) ParseTimestamp(string value)
        {
            var match = TimestampRegex.Match(value);
            if (!match.Success)
            {
                throw new InvalidOperationException($"Invalid Timestamp value: {value}");
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
                        throw new InvalidOperationException($"Invalid Timestamp value: {value}");
                    }
                    if (totalMinutes == 0 && sign == 1)
                    {
                        // This is an offset of -00:00, which means "unknown local offset". It makes no sense for a timestamp.
                        throw new InvalidOperationException($"Invalid Timestamp value: {value}");
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
                        throw new InvalidOperationException($"Invalid Timestamp value: {value}");
                    }
                }

                return (timestamp.Seconds, timestamp.Nanos);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException($"Invalid Timestamp value: {value}");
            }
        }

        private static bool IsNormalized(long seconds, int nanoseconds) =>
            nanoseconds >= 0 &&
            nanoseconds <= MaxNanos &&
            seconds >= UnixSecondsAtBclMinValue &&
            seconds <= UnixSecondsAtBclMaxValue;

        public static string GetTimestampText(int nanos, long seconds)
        {
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

                return builder.ToString();
            }
            else
            {
                throw new InvalidOperationException("Non-normalized timestamp value");
            }
        }

        public static (long seconds, int nanos) ParseDuration(string value)
        {
            var match = DurationRegex.Match(value);
            if (!match.Success)
            {
                throw new InvalidOperationException("Invalid Duration value: " + value);
            }
            var sign = match.Groups["sign"].Value;
            var secondsText = match.Groups["int"].Value;
            // Prohibit leading insignficant zeroes
            if (secondsText[0] == '0' && secondsText.Length > 1)
            {
                throw new InvalidOperationException("Invalid Duration value: " + value);
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
                    throw new InvalidOperationException("Invalid Duration value: " + value);
                }

                return (seconds, nanos);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid Duration value: " + value);
            }
        }

        public static string GetDurationText(int nanos, long seconds)
        {
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

                return builder.ToString();
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

        // Effectively a cache of mapping from enum values to the original name as specified in the proto file,
        // fetched by reflection.
        // The need for this is unfortunate, as is its unbounded size, but realistically it shouldn't cause issues.
        internal static class OriginalEnumValueHelper
        {
            private static readonly ConcurrentDictionary<Type, Dictionary<object, string>> _dictionaries
                = new ConcurrentDictionary<Type, Dictionary<object, string>>();

            internal static string? GetOriginalName(object value)
            {
                var enumType = value.GetType();
                Dictionary<object, string>? nameMapping;
                lock (_dictionaries)
                {
                    if (!_dictionaries.TryGetValue(enumType, out nameMapping))
                    {
                        nameMapping = GetNameMapping(enumType);
                        _dictionaries[enumType] = nameMapping;
                    }
                }

                string? originalName;
                // If this returns false, originalName will be null, which is what we want.
                nameMapping.TryGetValue(value, out originalName);
                return originalName;
            }

            private static Dictionary<object, string> GetNameMapping(Type enumType)
            {
                return enumType.GetTypeInfo().DeclaredFields
                    .Where(f => f.IsStatic)
                    .Where(f => f.GetCustomAttributes<OriginalNameAttribute>()
                                 .FirstOrDefault()?.PreferredAlias ?? true)
                    .ToDictionary(f => f.GetValue(null)!,
                                  f => f.GetCustomAttributes<OriginalNameAttribute>()
                                        .FirstOrDefault()
                                        // If the attribute hasn't been applied, fall back to the name of the field.
                                        ?.Name ?? f.Name);
            }
        }
    }
}
