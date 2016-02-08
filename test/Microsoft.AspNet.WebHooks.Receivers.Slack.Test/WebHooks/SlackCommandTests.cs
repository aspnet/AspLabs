// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SlackCommandParserTests
    {
        public static TheoryData<string, KeyValuePair<string, string>> ActionWithValueData
        {
            get
            {
                return new TheoryData<string, KeyValuePair<string, string>>
                {
                    { null, GetPair(string.Empty, string.Empty) },
                    { string.Empty, GetPair(string.Empty, string.Empty) },
                    { " ", GetPair(string.Empty, string.Empty) },
                    { "你好", GetPair("你好", string.Empty) },
                    { " 你好 ", GetPair("你好", string.Empty) },
                    { "你好 世界", GetPair("你好", "世界") },
                    { " 你好  世界 ", GetPair("你好", "世界") },
                    { "你好  世界", GetPair("你好", "世界") },
                    { "你好   世界", GetPair("你好", "世界") },
                    { "你 好 世 界", GetPair("你", "好 世 界") },
                    { "你 好;世\\界\\;", GetPair("你", "好;世\\界\\;") },
                    { " 你  好;世\\界\\; ", GetPair("你", "好;世\\界\\;") },
                };
            }
        }

        public static TheoryData<string, string> ValidParameterData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { "a=b;b=c", "a=b;b=c" },
                    { "a=b; b=c", "a=b; b=c" },
                    { "a=b ; b=c", "a=b ; b=c" },

                    { "a='';b=''", "a='';b=''" },
                    { "a=\"\";b=\"\"", "a=\"\";b=\"\"" },

                    { "a='b';b='c'", "a='b';b='c'" },
                    { "a=\"b\";b=\"c\"", "a=\"b\";b=\"c\"" },
                    { "a=';b';b='c;'", "a='\\\0b';b='c\\\0'" },
                    { "a=\";b\";b=\"c;\"", "a=\"\\\0b\";b=\"c\\\0\"" },

                    { "abcd", "abcd" },
                    { "'abcd'", "'abcd'" },
                    { "\"abcd\"", "\"abcd\"" },
                    { "ab\\;cd", "ab\\\0cd" },
                    { "'ab;cd'", "'ab\\\0cd'" },
                    { "\"ab;cd\"", "\"ab\\\0cd\"" },
                    { "'ab\\;cd'", "'ab\\\\\0cd'" },
                    { "\"ab\\;cd\"", "\"ab\\\\\0cd\"" },

                    { null, string.Empty },
                    { string.Empty, string.Empty },
                    { "''", "''" },
                    { "\"\"", "\"\"" },
                    { "\\;", "\\\0" },
                    { "';'", "'\\\0'" },
                    { "\";\"", "\"\\\0\"" },

                    { "\\;\\;\\;\\;", "\\\0\\\0\\\0\\\0" },
                    { "\\;\\;;\\;\\;", "\\\0\\\0;\\\0\\\0" },
                    { "';;;;'", "'\\\0\\\0\\\0\\\0'" },
                    { "\";;;;\"", "\"\\\0\\\0\\\0\\\0\"" },
                    { "';;';';;'", "'\\\0\\\0';'\\\0\\\0'" },
                    { "\";;\";\";;\"", "\"\\\0\\\0\";\"\\\0\\\0\"" },
                    { "'\\;\\;\\;\\;'", "'\\\\\0\\\\\0\\\\\0\\\\\0'" },
                    { "\"\\;\\;\\;\\;\"", "\"\\\\\0\\\\\0\\\\\0\\\\\0\"" },
                    { "'\\;\\;';'\\;\\;'", "'\\\\\0\\\\\0';'\\\\\0\\\\\0'" },
                    { "\"\\;\\;\";\"\\;\\;\"", "\"\\\\\0\\\\\0\";\"\\\\\0\\\\\0\"" },
                };
            }
        }

        public static TheoryData<string, char, int> InvalidParameterData
        {
            get
            {
                return new TheoryData<string, char, int>
                {
                    { "'", '\'', 0 },
                    { "\"", '"', 0 },
                    { "'''", '\'', 2 },
                    { "\"\"\"", '"', 2 },

                    { "a='b;b=c", '\'', 2 },
                    { "a=\"b;b=c", '"', 2 },
                    { "a='b;b=\"c\"", '\'', 2 },
                    { "a=\"b;b='c'", '"', 2 },
                    { "a='b;b='c'", '\'', 9 },
                    { "a=\"b;b=\"c\"", '"', 9 },
                    { "a=';b;b='c;'", '\'', 11 },
                    { "a=\";b;b=\"c;\"", '"', 11 },
                };
            }
        }

        public static TheoryData<string, string, string> ActionWithParametersData
        {
            get
            {
                return new TheoryData<string, string, string>
                {
                    { string.Empty, string.Empty, string.Empty },
                    { "action", "action", string.Empty },
                    { " action ", "action", string.Empty },
                    { "action p1=v1", "action", "p1=v1" },
                    { " action  p1 = v1 ", "action", "p1=v1" },
                    { "action p1=v1; p1=v2", "action", "p1=v1,v2" },
                    { "action p1=v1; p2=v2", "action", "p1=v1; p2=v2" },
                    { "action p1='v1'; p2=\"v2\"", "action", "p1=v1; p2=v2" },
                    { "action p1='v;1'; p2=\"v;2\"", "action", "p1=v;1; p2=v;2" },
                    { "action p1=\\;v1; p2=v\\;2; p3=v3\\;", "action", "p1=;v1; p2=v;2; p3=v3;" },
                    { "你好 世=界", "你好", "世=界" },
                };
            }
        }

        [Theory]
        [MemberData("ActionWithValueData")]
        public void ParseActionWithValue_HandlesInput(string text, KeyValuePair<string, string> expected)
        {
            // Act
            KeyValuePair<string, string> actual = SlackCommand.ParseActionWithValue(text);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("ActionWithParametersData")]
        public void ParseActionWithParameters_HandlesInput(string text, string expectedAction, string expectedParameters)
        {
            // Act
            KeyValuePair<string, NameValueCollection> actual = SlackCommand.ParseActionWithParameters(text);

            // Assert
            Assert.Equal(expectedAction, actual.Key);
            Assert.Equal(expectedParameters, actual.Value.ToString());
        }

        [Theory]
        [MemberData("ValidParameterData")]
        public void EncodeNonSeparatorCharacters_ParsesCorrectInput(string input, string expected)
        {
            // Act
            string actual = SlackCommand.EncodeNonSeparatorCharacters(input);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("'text'")]
        [InlineData("\"text\"")]
        public void ValidateParameterName_Throws_IfQuotedName(string name)
        {
            // Act
            ArgumentException ex = Assert.Throws<ArgumentException>(() => SlackCommand.ValidateParameterName(name));

            // Assert
            Assert.StartsWith(string.Format(CultureInfo.CurrentCulture, "Parameter name cannot be a quoted string: ({0}).", name), ex.Message);
        }

        [Theory]
        [InlineData("\\\0text")]
        [InlineData("te\\\0xt")]
        [InlineData("te\\\0\\\0xt")]
        [InlineData("text\\\0")]
        public void ValidateParameterName_Throws_IfInvalid(string name)
        {
            // Act
            ArgumentException ex = Assert.Throws<ArgumentException>(() => SlackCommand.ValidateParameterName(name));

            // Assert
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Parameter name cannot contain ';' characters: ({0}).", name.Replace("\\\0", ";")), ex.Message);
        }

        [Theory]
        [InlineData("text")]
        [InlineData("你 世界")]
        public void ValidateParameterName_Accepts_ValidName(string name)
        {
            SlackCommand.ValidateParameterName(name);
        }

        [Theory]
        [MemberData("InvalidParameterData")]

        public void EncodeNonSeparatorCharacters_Throws_IfInvalidInput(string input, char quote, int offset)
        {
            // Act
            ArgumentException ex = Assert.Throws<ArgumentException>(() => SlackCommand.EncodeNonSeparatorCharacters(input));

            // Assert
            Assert.Equal(string.Format(CultureInfo.CurrentCulture, "Unmatched quote ({0}) discovered at position {1}.", quote, offset), ex.Message);
        }

        private static KeyValuePair<string, string> GetPair(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }
    }
}
