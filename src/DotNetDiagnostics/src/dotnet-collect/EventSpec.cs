using System;
using System.Diagnostics.Tracing;
using System.Globalization;

namespace Microsoft.Diagnostics.Tools.Collect
{
    public class EventSpec
    {
        public string Provider { get; }
        public ulong Keywords { get; }
        public EventLevel Level { get; }

        public EventSpec(string provider, ulong keywords, EventLevel level)
        {
            Provider = provider;
            Keywords = keywords;
            Level = level;
        }

        public static bool TryParse(string input, out EventSpec spec)
        {
            spec = null;
            var splat = input.Split(':');

            if (splat.Length == 0)
            {
                return false;
            }

            var provider = splat[0];
            var keywords = ulong.MaxValue;
            var level = EventLevel.Verbose;

            if (splat.Length > 1)
            {
                if (!TryParseKeywords(splat[1], out keywords) && !TryParseLevel(splat[1], out level))
                {
                    return false;
                }
            }

            if (splat.Length > 2)
            {
                if (!TryParseLevel(splat[1], out level))
                {
                    return false;
                }
            }

            spec = new EventSpec(provider, keywords, level);
            return true;
        }

        public string ToConfigString() => $"{Provider}:0x{Keywords:X}:{(int)Level}";

        private static bool TryParseLevel(string input, out EventLevel level)
        {
            level = EventLevel.Verbose;
            if (int.TryParse(input, out var intLevel))
            {
                if (intLevel >= (int)EventLevel.LogAlways && intLevel <= (int)EventLevel.Verbose)
                {
                    level = (EventLevel)intLevel;
                    return true;
                }
            }
            else if (Enum.TryParse(input, ignoreCase: true, out level))
            {
                return true;
            }
            return false;
        }

        private static bool TryParseKeywords(string input, out ulong keywords)
        {
            if (input.StartsWith("0x"))
            {
                // Keywords
                if (ulong.TryParse(input, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out keywords))
                {
                    return true;
                }
            }

            keywords = ulong.MaxValue;
            return false;
        }
    }
}
