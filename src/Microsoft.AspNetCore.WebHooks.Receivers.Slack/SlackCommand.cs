// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.WebHooks.Properties;
using Microsoft.AspNetCore.WebHooks.Utilities;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    ///  The <see cref="SlackCommand"/> class provides mechanisms for parsing the text contained in
    ///  Slack slash commands and outgoing WebHooks following a variety of different formats enabling
    ///  different scenarios.
    /// </summary>
    public static class SlackCommand
    {
        private static readonly char[] EqualSeparator = new[] { '=' };
        private static readonly char[] LwsSeparator = new[] { ' ' };
        private static readonly char[] ParameterSeparator = new[] { ';' };

        /// <summary>
        /// Parses the 'text' of a slash command or 'subtext' of an outgoing WebHook of the form '<c>action value</c>'.
        /// </summary>
        /// <example>
        /// An example of an outgoing WebHook or slash command using this format is
        /// <c>'/assistant query what's the weather?'</c> where '/assistant' is the trigger word or slash command,
        /// 'query' is the action and 'what's the weather?' is the value.
        /// </example>
        /// <param name="text">The 'text' of a slash command or 'subtext' of an outgoing WebHook.</param>
        public static KeyValuePair<string, string> ParseActionWithValue(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new KeyValuePair<string, string>(string.Empty, string.Empty);
            }

            var values = new TrimmingTokenizer(text, LwsSeparator, 2).ToArray();
            if (values.Length == 0)
            {
                return new KeyValuePair<string, string>(string.Empty, string.Empty);
            }

            var result = new KeyValuePair<string, string>(
                values[0].Value,
                values.Length > 1 ? values[1].Value : string.Empty);

            return result;
        }

        // ??? Should we continue to use NameValueCollection here and as the base for ParameterCollection?
        /// <summary>
        /// Parses the 'text' of a slash command or 'subtext' of an outgoing WebHook of the form
        /// '<c>action param1; param2=value2; param3=value 3; param4="quoted value4"; ...</c>'.
        /// Parameter values containing semi-colons can either escape the semi-colon using a backslash character,
        /// i.e '\;', or quote the value using single quotes or double quotes.
        /// </summary>
        /// <example>
        /// An example of an outgoing WebHook or slash command using this format is
        /// <c>/appointment add title=doctor visit; time=Feb 3 2016 2 PM; location=Children's Hospital</c>
        /// where '/appointment' is the trigger word or slash command, 'add' is the action and title, time, and location
        /// are parameters.
        /// </example>
        /// <param name="text">The 'text' of a slash command or 'subtext' of an outgoing WebHook.</param>
        public static KeyValuePair<string, NameValueCollection> ParseActionWithParameters(string text)
        {
            var actionValue = ParseActionWithValue(text);

            var parameters = new ParameterCollection();
            var result = new KeyValuePair<string, NameValueCollection>(actionValue.Key, parameters);

            var encodedSeparators = EncodeNonSeparatorCharacters(actionValue.Value);
            var keyValuePairs = new TrimmingTokenizer(encodedSeparators, ParameterSeparator);
            foreach (var keyValuePair in keyValuePairs)
            {
                var parameter = new TrimmingTokenizer(keyValuePair, EqualSeparator, 2).ToArray();

                var name = parameter[0].Value;
                ValidateParameterName(name);

                // Unquote and convert parameter value
                var value = string.Empty;
                if (parameter.Length > 1)
                {
                    value = parameter[1].Value;
                    value = value.Trim(new[] { '\'' }).Trim('"');
                    value = value.Replace("\\\0", ";");
                }

                parameters.Add(name, value);
            }

            return result;
        }

        /// <summary>
        /// Verify that parameter name is not a quoted string and that it doesn't contain encoded ';' characters.
        /// </summary>
        internal static void ValidateParameterName(string name)
        {
            if (name.Length > 0 && (name[0] == '\'' || name[0] == '"'))
            {
                var message = string.Format(CultureInfo.CurrentCulture, Resources.Command_NameIsQuotedString, name);
                throw new ArgumentException(message);
            }

            if (name.IndexOf("\\\0", StringComparison.Ordinal) > -1)
            {
                name = name.Replace("\\\0", ";");
                var message = string.Format(CultureInfo.CurrentCulture, Resources.Command_NameInvalid, name);
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Transform quoted or escaped ';' characters so that we can split the stream on actual
        /// parameter separators.
        /// </summary>
        internal static string EncodeNonSeparatorCharacters(string text)
        {
            if (text == null || text.Length == 0)
            {
                return string.Empty;
            }

            var normalized = new StringBuilder(text.Length);
            var bytesConsumed = 0;
            while (true)
            {
                // Look for starting quote (either single or double)
                if (text[bytesConsumed] == '\'' || text[bytesConsumed] == '"')
                {
                    var quoteOffset = bytesConsumed;
                    var quote = text[bytesConsumed];
                    normalized.Append(quote);
                    if (++bytesConsumed == text.Length)
                    {
                        var message = string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.Command_ContainsUnmatchedQuote,
                            quote,
                            quoteOffset);
                        throw new ArgumentException(message);
                    }

                    // Look for matching closing quote while encoding ';' on the way
                    while (text[bytesConsumed] != quote)
                    {
                        // Encode semicolon
                        var ch = text[bytesConsumed];
                        if (ch == ';')
                        {
                            normalized.Append("\\\0");
                        }
                        else
                        {
                            normalized.Append(ch);
                        }

                        if (++bytesConsumed == text.Length)
                        {
                            var message = string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Command_ContainsUnmatchedQuote,
                                quote,
                                quoteOffset);
                            throw new ArgumentException(message);
                        }
                    }

                    // Record and move past closing quote
                    normalized.Append(text[bytesConsumed]);
                    if (++bytesConsumed == text.Length)
                    {
                        return normalized.ToString();
                    }
                }
                else
                {
                    var ch = text[bytesConsumed];
                    if (ch != ';' || (bytesConsumed > 0 && text[bytesConsumed - 1] != '\\'))
                    {
                        normalized.Append(ch);
                    }
                    else
                    {
                        normalized.Append('\0');
                    }

                    if (++bytesConsumed == text.Length)
                    {
                        return normalized.ToString();
                    }
                }
            }
        }
    }
}
