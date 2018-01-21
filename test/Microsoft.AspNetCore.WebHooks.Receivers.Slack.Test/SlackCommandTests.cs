// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks
{
    public class SlackCommandTests
    {
        public static TheoryData<string, KeyValuePair<StringSegment, StringSegment>> ActionWithValueData
        {
            get
            {
                return new TheoryData<string, KeyValuePair<StringSegment, StringSegment>>
                {
                    { null, new KeyValuePair<StringSegment, StringSegment>(string.Empty, string.Empty) },
                    { string.Empty, new KeyValuePair<StringSegment, StringSegment>(string.Empty, string.Empty) },
                    { " ", new KeyValuePair<StringSegment, StringSegment>(string.Empty, string.Empty) },
                    { "你好", new KeyValuePair<StringSegment, StringSegment>("你好", string.Empty) },
                    { " 你好 ", new KeyValuePair<StringSegment, StringSegment>("你好", string.Empty) },
                    { "你好 世界", new KeyValuePair<StringSegment, StringSegment>("你好", "世界") },
                    { " 你好  世界 ", new KeyValuePair<StringSegment, StringSegment>("你好", "世界") },
                    { "你好  世界", new KeyValuePair<StringSegment, StringSegment>("你好", "世界") },
                    { "你好   世界", new KeyValuePair<StringSegment, StringSegment>("你好", "世界") },
                    { "你 好 世 界", new KeyValuePair<StringSegment, StringSegment>("你", "好 世 界") },
                    { "你 好;世\\界\\;", new KeyValuePair<StringSegment, StringSegment>("你", "好;世\\界\\;") },
                    { " 你  好;世\\界\\; ", new KeyValuePair<StringSegment, StringSegment>("你", "好;世\\界\\;") },
                };
            }
        }

        public static TheoryData<string, string> RoundTripData
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { null, string.Empty },
                    { string.Empty, string.Empty },
                    { "p1=v1", "p1=v1" },
                    { "  p1 = v1 ", "p1=v1" },
                    { "p1=v1; p1=v2", "p1=v1; p1=v2" },
                    { "p1=v1    ;p2=v2", "p1=v1; p2=v2" },
                    { "p1=v'1    ;p2=v\"2", "p1=v'1; p2=v\"2" },
                    { "p1=' v1'; p2=\" v2\"", "p1=' v1'; p2=' v2'" },
                    { "p1='v1 ';        p2=\"v2 \"", "p1='v1 '; p2='v2 '" },
                    { "p1=v  1           ; p2=\"v  2\"   ", "p1=v  1; p2=v  2" },
                    { "p1=' v1 '; p2=\" v2 \"", "p1=' v1 '; p2=' v2 '" },
                    { "p1=' v\"1 '; p2=\" v'2 \"", "p1=' v\"1 '; p2=\" v'2 \"" },
                    { "p1='v;1'; p2=\"v;2\"", "p1=v\\;1; p2=v\\;2" },
                    { "p1=\\;v1; p2=v\\;2; p3=v3\\;", "p1=\\;v1; p2=v\\;2; p3=v3\\;" },
                    { "p1=\\;v1; p2=v\\;2; p1=v3\\;", "p1=\\;v1; p1=v3\\;; p2=v\\;2" },
                    { " 世=界", "世=界" },

                    { "a=b;b=c", "a=b; b=c" },
                    { "a=b; b=c", "a=b; b=c" },
                    { "a=  b ; b=c", "a=b; b=c" },

                    { "a;b", "a; b" },
                    { "a;  b", "a; b" },
                    { "a=;b=", "a; b" },
                    { "a = ; b = ", "a; b" },
                    { "a='';b=''", "a; b" },
                    { "a=\"\";b=\"\"", "a; b" },
                    { "a=  '';b=''", "a; b" },
                    { "a=  \"\"  ;  b=\"\"", "a; b" },

                    { "a='b';b='c'", "a=b; b=c" },
                    { "a=\"b\";b=\"c\"", "a=b; b=c" },
                    { "a=';b';b='c;'", "a=\\;b; b=c\\;" },
                    { "a=\";b\";b=\"c;\"", "a=\\;b; b=c\\;" },

                    { "abcd", "abcd" },
                    { "a=abcd", "a=abcd" },
                    { "a='abcd'", "a=abcd" },
                    { "a=\"abcd\"", "a=abcd" },
                    { "a=ab\\;cd", "a=ab\\;cd" },
                    { "a='ab;cd'", "a=ab\\;cd" },
                    { "a=\"ab;cd\"", "a=ab\\;cd" },
                    { "a='ab\\;cd'", "a=ab\\\\;cd" },
                    { "a=\"ab\\;cd\"", "a=ab\\\\;cd" },

                    { "a=''", "a" },
                    { "a=\"\"", "a" },
                    { "a=\\;", "a=\\;" },
                    { "a=';'", "a=\\;" },
                    { "a=\";\"", "a=\\;" },

                    { "a=\\;\\;\\;\\;", "a=\\;\\;\\;\\;" },
                    { "a=\\;\\;;b=\\;\\;", "a=\\;\\;; b=\\;\\;" },
                    { "a=';;;;'", "a=\\;\\;\\;\\;" },
                    { "a=\";;;;\"", "a=\\;\\;\\;\\;" },
                    { "a=';;';b=';;'", "a=\\;\\;; b=\\;\\;" },
                    { "a=\";;\";b=\";;\"", "a=\\;\\;; b=\\;\\;" },
                    { "a='\\;\\;\\;\\;'", "a=\\\\;\\\\;\\\\;\\\\;" },
                    { "a=\"\\;\\;\\;\\;\"", "a=\\\\;\\\\;\\\\;\\\\;" },
                    { "a='\\;\\;';b='\\;\\;'", "a=\\\\;\\\\;; b=\\\\;\\\\;" },
                    { "a=\"\\;\\;\";b=\"\\;\\;\"", "a=\\\\;\\\\;; b=\\\\;\\\\;" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(ActionWithValueData))]
        public void ParseActionWithValue_HandlesInput(string text, KeyValuePair<StringSegment, StringSegment> expected)
        {
            // Arrange & Act
            var actual = SlackCommand.ParseActionWithValue(text);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("text")]
        [InlineData("  text")]
        [InlineData("\\text")]
        [InlineData("  text\\")]
        [InlineData("  text\\ text  ")]
        [InlineData("你 世界")]
        [InlineData("  你 世界  ")]
        public void ParseParameters_AcceptsValidName(string name)
        {
            // Arrange & Act
            var expectedName = name.Trim();
            var result = SlackCommand.TryParseParameters(name, out var error);

            // Assert
            var keyValuePair = Assert.Single(result);
            Assert.True(keyValuePair.Key.HasValue);
            Assert.Equal(expectedName, keyValuePair.Key.Value);

            var segment = Assert.Single(keyValuePair.Value);
            Assert.True(StringSegment.IsNullOrEmpty(segment));

            Assert.Null(error);
        }

        [Theory]
        [InlineData("text", "text")]
        [InlineData("  text", "text")]
        [InlineData("你 世界", "你 世界")]
        [InlineData("  你 世界  ", "你 世界")]
        [InlineData("'text'", "text")]
        [InlineData("  'text'", "text")]
        [InlineData("  '  text'", "  text")]
        [InlineData("  'text  '", "text  ")]
        [InlineData("  '  text  '", "  text  ")]
        [InlineData("'你 世界'", "你 世界")]
        [InlineData("  '你 世界  '", "你 世界  ")]
        [InlineData("\"text\"", "text")]
        [InlineData("  \"text\"", "text")]
        [InlineData("  \"  text\"", "  text")]
        [InlineData("  \"text  \"", "text  ")]
        [InlineData("  \"  text  \"", "  text  ")]
        [InlineData("\"你 世界\"", "你 世界")]
        [InlineData("  \"你 世界  \"", "你 世界  ")]
        [InlineData("text'text", "text'text")]
        [InlineData("text \" text", "text \" text")]
        [InlineData("  text \" text  ", "text \" text")]
        [InlineData("\\", "\\")]
        [InlineData("\\;", ";")]
        [InlineData("\\\\;", "\\;")]
        [InlineData("\\;\\", ";\\")]
        [InlineData("  \\;  ", ";")]
        [InlineData("';'", ";")]
        [InlineData("  ';;'  ", ";;")]
        [InlineData("'  ;;;  '", "  ;;;  ")]
        [InlineData("'  ;\";  '", "  ;\";  ")]
        [InlineData("\";\"", ";")]
        [InlineData("  \";;\"  ", ";;")]
        [InlineData("\"  ;;;  \"", "  ;;;  ")]
        [InlineData("\"  ;';  \"", "  ;';  ")]
        public void ParseParameters_AcceptsValidValue(string value, string expectedValue)
        {
            // Arrange & Act
            var result = SlackCommand.TryParseParameters($"name={value}", out var error);

            // Assert
            var keyValuePair = Assert.Single(result);
            Assert.True(keyValuePair.Key.HasValue);
            Assert.Equal("name", keyValuePair.Key.Value);

            var segment = Assert.Single(keyValuePair.Value);
            Assert.True(segment.HasValue);
            Assert.Equal(expectedValue, segment.Value);

            Assert.Null(error);
        }

        [Theory]
        [MemberData(nameof(RoundTripData))]
        public void ParseParameters_GetNormalizedParameterString_RoundTrips(string text, string expectedParameters)
        {
            // Arrange & Act
            var result = SlackCommand.TryParseParameters(text, out var error);
            var actual = SlackCommand.GetNormalizedParameterString(result);

            // Assert
            Assert.Null(error);
            Assert.Equal(expectedParameters, actual);
        }

        [Theory]
        [InlineData("'text'", '\'', 0)]
        [InlineData("\"text\"", '"', 0)]
        [InlineData("   \"text\"", '"', 3)]
        public void ParseParameters_ReturnsError_IfQuotedName(string name, char quote, int offset)
        {
            // Arrange
            var expected = $"Parameter name cannot be a quoted string. Unexpected character ({quote}) " +
                $"discovered at position {offset}.";

            // Act
            var result = SlackCommand.TryParseParameters(name, out var error);

            // Assert
            Assert.Null(result);
            Assert.StartsWith(expected, error);
        }

        [Theory]
        [InlineData("\\;text", 0)]
        [InlineData("te\\;xt", 2)]
        [InlineData("te\\;\\;xt", 2)]
        [InlineData("text\\;", 4)]
        [InlineData("  \\;text  ", 2)]
        [InlineData("  te\\;xt", 4)]
        [InlineData("  text\\;  ", 6)]
        public void ParseParameters_ReturnsError_IfInvalidName(string name, int offset)
        {
            // Arrange
            var expected = "Parameter name cannot contain ';' characters. Unexpected escape sequence (\\;) " +
                $"discovered at position {offset}.";

            // Act
            var result = SlackCommand.TryParseParameters(name, out var error);

            // Assert
            Assert.Null(result);
            Assert.Equal(expected, error);
        }

        [Theory]
        [InlineData("a='", '\'', 2 )]
        [InlineData("a=\"", '"', 2 )]
        [InlineData("  a=\"", '"', 4)]
        [InlineData("a='b;b=c", '\'', 2)]
        [InlineData("  a='b;  b=c", '\'', 4)]
        [InlineData("a=\"b;b=c", '"', 2)]
        [InlineData("a='b;b=\"c\"", '\'', 2)]
        [InlineData("a=\"b;b='c'", '"', 2)]
        [InlineData("   a=\"b;  b='c'  ", '"', 5)]
        public void ParseParameters_ReturnsError_IfValueContainsMismatchedQuote(string input, char quote, int offset)
        {
            // Arrange
            var expected = $"Unmatched quote ({quote}) discovered at position {offset}.";

            // Act
            var result = SlackCommand.TryParseParameters(input, out var error);

            // Assert
            Assert.Null(result);
            Assert.Equal(expected, error);
        }

        [Theory]
        [InlineData("a='''", '\'', 4)]
        [InlineData("a=\"\"\"", '"', 4)]
        [InlineData("  a=\"\"\"", '"', 6)]
        [InlineData("a='b;b='c'", 'c', 8)]
        [InlineData("  a='b;b='  c'", 'c', 12)]
        [InlineData("a=\"b;b=\"c\"", 'c', 8)]
        [InlineData("a=';b;b='c;'", 'c', 9)]
        [InlineData("a=\";b;b=\"c;\"", 'c', 9)]
        [InlineData("  a=\";b;b=\" c; \"", 'c', 12)]
        public void ParseParameters_ReturnsError_IfTextFoundAfterQuotedString(string input, char ch, int offset)
        {
            // Arrange
            var expected = $"Parameter value contains text after a quoted string. Unexpected character ({ch}) " +
                $"discovered at position {offset}.";

            // Act
            var result = SlackCommand.TryParseParameters(input, out var error);

            // Assert
            Assert.Null(result);
            Assert.Equal(expected, error);
        }
    }
}
