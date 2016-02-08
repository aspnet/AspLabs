// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    ///  The <see cref="SlackCommand"/> class provides mechanisms for parsing the text contained in
    ///  Slack slash commands and outgoing WebHooks following a variety of different formats enabling
    ///  different scenarios. 
    /// </summary>
    public static class SlackCommand
    {
        private static readonly char[] LwsSeparator = new[] { ' ' };
        private static readonly char[] EqualSeparator = new[] { '=' };

        /// <summary>
        /// Parses the 'text' of a slash command or 'subtext' of an outgoing WebHook of the form '<c>action value</c>'.
        /// </summary>
        /// <example>
        /// An example of an outgoing WebHook or slash command using this format is '/assistant query what's the weather?'
        /// where '/assistant' is the trigger word or slash command, 'query' is the action and 'what's the weather?' is the value.
        /// </example>
        /// <param name="text">The 'text' of a slash command or 'subtext' of an outgoing WebHook.</param>
        public static KeyValuePair<string, string> ParseActionWithValue(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new KeyValuePair<string, string>(string.Empty, string.Empty);
            }

            string[] values = text.Split(LwsSeparator, 2, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (values.Length == 0)
            {
                return new KeyValuePair<string, string>(string.Empty, string.Empty);
            }
            var result = new KeyValuePair<string, string>(values[0], values.Length > 1 ? values[1] : string.Empty);
            return result;
        }

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
            KeyValuePair<string, string> actionValue = ParseActionWithValue(text);

            ParameterCollection parameters = new ParameterCollection();
            var result = new KeyValuePair<string, NameValueCollection>(actionValue.Key, parameters);

            string encodedSeparators = EncodeNonSeparatorCharacters(actionValue.Value);
            string[] keyValuePairs = encodedSeparators.SplitAndTrim(';');
            foreach (string p in keyValuePairs)
            {
                string[] parameter = p.Split(EqualSeparator, 2, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

                string name = parameter[0];
                ValidateParameterName(name);

                // Unquote and convert parameter value
                string value = string.Empty;
                if (parameter.Length > 1)
                {
                    value = parameter[1];
                    value = value.Trim(new[] { '\'' }).Trim('"');
                    value = value.Replace("\\\0", ";");
                }
                parameters.Add(name, value);
            }

            return result;
        }

        /// <summary>
        /// Verify that parameter name is not a quoted string and that it doesn't contain ';'.
        /// </summary>
        internal static void ValidateParameterName(string name)
        {
            if (name.Length > 0 && (name[0] == '\'' || name[0] == '"'))
            {
                string msg = string.Format(CultureInfo.CurrentCulture, SlackReceiverResources.Receiver_CommandNameQuotedString, name);
                throw new ArgumentException(msg);
            }
            if (name.IndexOf("\\\0", StringComparison.Ordinal) > -1)
            {
                name = name.Replace("\\\0", ";");
                string msg = string.Format(CultureInfo.CurrentCulture, SlackReceiverResources.Receiver_CommandNameInvalid, name);
                throw new ArgumentException(msg);
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

            StringBuilder normalized = new StringBuilder(text.Length);
            int bytesConsumed = 0;
            while (true)
            {
                // Look for starting quote (either single or double)
                if (text[bytesConsumed] == '\'' || text[bytesConsumed] == '"')
                {
                    int quoteOffset = bytesConsumed;
                    char quote = text[bytesConsumed];
                    normalized.Append(quote);
                    if (++bytesConsumed == text.Length)
                    {
                        string msg = string.Format(CultureInfo.CurrentCulture, SlackReceiverResources.Receiver_CommandUnmatchedQuote, quote, quoteOffset);
                        throw new ArgumentException(msg);
                    }

                    // Look for matching closing quote while encoding ';' on the way
                    while (text[bytesConsumed] != quote)
                    {
                        // Encode semicolon
                        char ch = text[bytesConsumed];
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
                            string msg = string.Format(CultureInfo.CurrentCulture, SlackReceiverResources.Receiver_CommandUnmatchedQuote, quote, quoteOffset);
                            throw new ArgumentException(msg);
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
                    char ch = text[bytesConsumed];
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
