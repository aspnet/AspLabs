// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    ///  The <see cref="SlackCommand"/> class provides mechanisms for parsing the text contained in
    ///  Slack slash commands and outgoing WebHooks following a variety of different formats enabling
    ///  different scenarios.
    /// </summary>
    public static class SlackCommand
    {
        private static readonly char[] LwsSeparator = new[] { ' ' };

        /// <summary>
        /// Parses the 'text' of a slash command or 'subtext' of an outgoing WebHook of the form '<c>action value</c>'.
        /// </summary>
        /// <example>
        /// An example of an outgoing WebHook or slash command using this format is
        /// <c>'/assistant query what's the weather?'</c> where '/assistant' is the trigger word or slash command,
        /// 'query' is the action and 'what's the weather?' is the value.
        /// </example>
        /// <param name="text">The 'text' of a slash command or 'subtext' of an outgoing WebHook.</param>
        /// <returns>
        /// A <see cref="KeyValuePair{TKey, TValue}"/> mapping the action to its value. Both properties of the
        /// <see cref="KeyValuePair{TKey, TValue}"/> will be <see cref="StringSegment.Empty"/> when the
        /// <paramref name="text"/> does not contain an action. The <see cref="KeyValuePair{TKey, TValue}.Value"/> will
        /// be <see cref="StringSegment.Empty"/> when the <paramref name="text"/> contains no interior spaces i.e. the
        /// action has no value.
        /// </returns>
        public static KeyValuePair<StringSegment, StringSegment> ParseActionWithValue(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new KeyValuePair<StringSegment, StringSegment>(StringSegment.Empty, StringSegment.Empty);
            }

            var values = new TrimmingTokenizer(text, LwsSeparator, maxCount: 2);
            var enumerator = values.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return new KeyValuePair<StringSegment, StringSegment>(StringSegment.Empty, StringSegment.Empty);
            }

            var action = enumerator.Current;
            var value = enumerator.MoveNext() ? enumerator.Current : StringSegment.Empty;

            return new KeyValuePair<StringSegment, StringSegment>(action, value);
        }

        /// <summary>
        /// Parses the parameters a slash command or of an outgoing WebHook of the form
        /// '<c>action param1; param2=value2; param3=value 3; param4="quoted value4"; ...</c>'.
        /// Parameter values containing semi-colons can either escape the semi-colon using a backslash character,
        /// i.e '\;', or quote the value using single quotes or double quotes.
        /// </summary>
        /// <example>
        /// An example of an outgoing WebHook or slash command using this format is
        /// <c>/appointment add title=doctor visit; time=Feb 3 2016 2 PM; location=Children's Hospital</c>
        /// where '/appointment' is the trigger word or slash command, 'add' is the action and title, time, and
        /// location are parameters. In this case, caller would pass
        /// <c>title=doctor visit; time=Feb 3 2016 2 PM; location=Children's Hospital</c> to this method.
        /// </example>
        /// <param name="text">
        /// The parameter portion of a slash command or an outgoing WebHook. Often the
        /// <see cref="KeyValuePair{StringSegment, StringSegment}.Value"/> from what
        /// <see cref="ParseActionWithValue"/> returned.
        /// </param>
        /// <returns>
        /// A <see cref="ValueTuple{T1, T2}"/> in which one property is always <see langword="null"/>. When successful,
        /// <c>Error</c> is <see langword="null"/> and <c>Parameters</c> an <see cref="IDictionary{TKey, TValue}"/>
        /// mapping zero or more parameter names to zero or more values. Otherwise, <c>Parameters</c> is
        /// <see langword="null"/> and <c>Error</c> a <see cref="string"/> describing the specific problem.
        /// </returns>
        public static (IDictionary<StringSegment, IList<StringSegment>> Parameters, string Error) ParseParameters(
            StringSegment text)
        {
            var parameters = new Dictionary<StringSegment, IList<StringSegment>>(
                StringSegmentComparer.OrdinalIgnoreCase);
            var index = GetWhitespaceLength(text, initialIndex: 0);
            while (index < text.Length)
            {
                var (length, name, error) = GetParameterName(text, index);
                if (error != null)
                {
                    return (Parameters: null, error);
                }

                index += length;

                var equalsLength = GetEqualsLength(text, index);
                index += equalsLength;

                var value = StringSegment.Empty;
                if (equalsLength > 0)
                {
                    (length, value, error) = GetParameterValue(text, index);
                    if (error != null)
                    {
                        return (Parameters: null, error);
                    }

                    index += length;
                }

                if (!(StringSegment.IsNullOrEmpty(name) && StringSegment.IsNullOrEmpty(value)))
                {
                    if (!parameters.TryGetValue(name, out var list))
                    {
                        list = new List<StringSegment>();
                        parameters.Add(name, list);
                    }

                    list.Add(value);
                }

                index += GetSemicolonLength(text, index);
            }

            return (parameters, Error: null);
        }

        /// <summary>
        /// <para>
        /// Returns a normalized representation of the given <paramref name="parameters"/>. This method and
        /// <see cref="ParseParameters"/> round-trip the semantics of the original text.
        /// </para>
        /// <para>
        /// Does not preserve syntax such as whitespace in the <see cref="StringSegment"/> passed to
        /// <see cref="ParseParameters"/>, how parameter values were quoted, or the parameter order.
        /// </para>
        /// </summary>
        /// <param name="parameters">The mapping of parameter names to zero or more parameter values.</param>
        /// <returns>A normalized representation of the given <paramref name="parameters"/>.</returns>
        public static string GetNormalizedParameterString(IDictionary<StringSegment, IList<StringSegment>> parameters)
        {
            if (parameters.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var addSemicolon = false;
            foreach (var parameter in parameters)
            {
                foreach (var parameterValue in parameter.Value)
                {
                    if (addSemicolon)
                    {
                        builder.Append("; ");
                    }

                    addSemicolon = true;
                    builder.Append(parameter.Key.Value);
                    if (StringSegment.IsNullOrEmpty(parameterValue))
                    {
                        continue;
                    }

                    builder.Append('=');
                    if (StringSegment.Equals(parameterValue, parameterValue.Trim(), StringComparison.Ordinal))
                    {
                        // No leading or trailing whitespace. Escape semicolons in value using backslashes.
                        for (var i = 0; i < parameterValue.Length; i++)
                        {
                            var ch = parameterValue[i];
                            if (ch == ';')
                            {
                                builder.Append('\\');
                            }

                            builder.Append(ch);
                        }
                    }
                    else if (parameterValue.IndexOf('\'') == -1)
                    {
                        // Need surrounding quotes and value does not contain single quotes. Use single quotes.
                        builder.Append('\'');
                        builder.Append(parameterValue.Value);
                        builder.Append('\'');
                    }
                    else
                    {
                        // Need quotes and value contains single quotes. Use double quotes. (No way to escape quotes.)
                        builder.Append('"');
                        builder.Append(parameterValue.Value);
                        builder.Append('"');
                    }
                }
            }

            return builder.ToString();
        }

        // Length is the number of characters required to reach equals separating a parameter name from its value,
        // semicolon separating one parameter from another, or end of the given segment.
        private static (int Length, StringSegment Value, string Error) GetParameterName(
            StringSegment segment,
            int initialIndex)
        {
            var index = initialIndex;
            for (; index < segment.Length; index++)
            {
                // Names can be pretty much anything but must not contain quotes or an escaped backslash. Treated as
                // StringSegment.Empty if only whitespace appears before the equals or semicolon.
                var done = false;
                switch (segment[index])
                {
                    case '\'':
                    case '"':
                        {
                            var error = string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Command_NameIsQuotedString,
                                segment[index],
                                index);
                            return (Length: 0, StringSegment.Empty, error);
                        }

                    case '=':
                    case ';':
                        done = true;
                        break;

                    case '\\':
                        if (index < segment.Length - 1 && segment[index + 1] == ';')
                        {
                            // An attempt to include a quoted semicolon in a name. Message refers to escape sequence.
                            var error = string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Command_NameInvalid,
                                segment.Substring(index, 2),
                                index);
                            return (Length: 0, StringSegment.Empty, error);
                        }

                        // Nothing else to do: Backslashes just backslashes unless quoting semicolons outside a string.
                        break;
                }

                if (done)
                {
                    break;
                }
            }

            var name = segment.Subsegment(initialIndex, index - initialIndex).TrimEnd();

            return (index - initialIndex, name, Error: null);
        }

        // Return number of characters required to reach start of the parameter value, semicolon separating one
        // parameter from another, or end of the given segment.
        private static int GetEqualsLength(StringSegment segment, int initialIndex)
        {
            if (initialIndex >= segment.Length || segment[initialIndex] != '=')
            {
                return 0;
            }

            return GetWhitespaceLength(segment, initialIndex + 1) + 1;
        }

        // Return number of characters required to reach start of the next parameter or end of the given segment.
        private static int GetSemicolonLength(StringSegment segment, int initialIndex)
        {
            if (initialIndex >= segment.Length || segment[initialIndex] != ';')
            {
                return 0;
            }

            return GetWhitespaceLength(segment, initialIndex + 1) + 1;
        }

        // Length is the number of characters required to reach semicolon separating one parameter from another
        // or end of the given segment.
        private static (int Length, StringSegment Value, string Error) GetParameterValue(
            StringSegment segment,
            int initialIndex)
        {
            if (initialIndex >= segment.Length)
            {
                return (Length: 0, StringSegment.Empty, Error: null);
            }

            var firstChar = segment[initialIndex];
            if (firstChar == '\'' || firstChar == '"')
            {
                var quoteIndex = segment.IndexOf(firstChar, initialIndex + 1);
                if (quoteIndex == -1)
                {
                    var error = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Command_ContainsUnmatchedQuote,
                        firstChar,
                        initialIndex);
                    return (Length: 0, StringSegment.Empty, error);
                }

                var index = quoteIndex + 1;
                index += GetWhitespaceLength(segment, index);

                if (index < segment.Length && segment[index] != ';')
                {
                    // Found something other than whitespace before the next semicolon or the end of the segment.
                    var error = string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.Command_ValueInvalid,
                        segment[index],
                        index);
                    return (Length: 0, StringSegment.Empty, error);
                }

                var value = segment.Subsegment(initialIndex + 1, length: quoteIndex - initialIndex - 1);
                return (index - initialIndex, value, Error: null);
            }

            var (length, unquotedValue) = GetUnquotedValue(segment, initialIndex);
            return (length, unquotedValue, Error: null);
        }

        private static (int Length, StringSegment Value) GetUnquotedValue(StringSegment segment, int initialIndex)
        {
            var backslashesToRemove = 0;
            var index = initialIndex;
            for (; index < segment.Length; index++)
            {
                var done = false;
                switch (segment[index])
                {
                    case ';':
                        done = true;
                        break;

                    case '\\':
                        if (index < segment.Length - 1 && segment[index + 1] == ';')
                        {
                            backslashesToRemove++;
                            index++;
                        }
                        break;
                }

                if (done)
                {
                    break;
                }
            }

            var length = index - initialIndex;
            StringSegment value;
            if (backslashesToRemove == 0)
            {
                value = segment.Subsegment(initialIndex, length);
            }
            else
            {
                var builder = new StringBuilder(length - backslashesToRemove);
                for (var i = initialIndex; i < index; i++)
                {
                    // Append all characters except a backslash before a semicolon.
                    var ch = segment[i];
                    if (!(ch == '\\' && i < index - 1 && segment[i + 1] == ';'))
                    {
                        builder.Append(ch);
                    }
                }

                value = new StringSegment(builder.ToString());
            }

            return (length, value.TrimEnd());
        }

        private static int GetWhitespaceLength(StringSegment segment, int initialIndex)
        {
            var index = initialIndex;
            for (; index < segment.Length && char.IsWhiteSpace(segment[index]); index++)
            {
            }

            return index - initialIndex;
        }
    }
}
