// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.Utilities
{
    public class TrimmingTokenizerTests
    {
        public static TheoryData<string, char[], int> CountData
        {
            get
            {
                return new TheoryData<string, char[], int>
                {
                    { string.Empty, new[] { ',' }, 0 },
                    { "   ", new[] { ',' }, 0 },
                    { ",,,", new[] { ',' }, 0 },
                    { "   , , ,  ", new[] { ',' }, 0 },
                    { "a,b,c", Array.Empty<char>(), 1 },
                    { "a, b, c", Array.Empty<char>(), 1 },
                    { ",,,a, ,,,b, c,,,", Array.Empty<char>(), 1 },
                    { " , , , a, , , , b, c, , , ", Array.Empty<char>(), 1 },
                    { " , , , a, , , , b, c, , , ", new[] { ' ' }, 11 },
                    { "a,b,c", new[] { ',' }, 3 },
                    { " a , b , c ", new[] { ',' }, 3 },
                    { ",,,a, ,,,b, c,,,", new[] { ',' }, 3 },
                    { " , , , a, , , , b, c, , , ", new[] { ',' }, 3 },
                    { ",;,a, ,;,b, c,;,", new[] { ',', ';' }, 3 },
                    { " , ; , a, ; , , b, c, ; , ", new[] { ',', ';' }, 3 },
                    { ";,;a; ;,;b; c;,;", new[] { ',', ';' }, 3 },
                    { " ; , ; a; , ; ; b; c; , ; ", new[] { ',', ';' }, 3 },
                    { "a, b, c", new[] { '_' }, 1 },
                    { "你,,好,, 世, ,\t,\r\n , 界", new[] { ',' }, 4 },
                };
            }
        }

        public static TheoryData<string, char[], int> CountMax2Data
        {
            get
            {
                return new TheoryData<string, char[], int>
                {
                    { string.Empty, new[] { ',' }, 0 },
                    { "   ", new[] { ',' }, 0 },
                    { ",,,", new[] { ',' }, 0 },
                    { "   , , ,  ", new[] { ',' }, 0 },
                    { "a,b,c", Array.Empty<char>(), 1 },
                    { "a, b, c", Array.Empty<char>(), 1 },
                    { ",,,a, ,,,b, c,,,", Array.Empty<char>(), 1 },
                    { " , , , a, , , , b, c, , , ", Array.Empty<char>(), 1 },
                    { " , , , a, , , , b, c, , , ", new[] { ' ' }, 2 },
                    { "a,b,c", new[] { ',' }, 2 },
                    { " a , b , c ", new[] { ',' }, 2 },
                    { ",,,a, ,,,b, c,,,", new[] { ',' }, 2 },
                    { " , , , a, , , , b, c, , , ", new[] { ',' }, 2 },
                    { ",;,a, ,;,b, c,;,", new[] { ',', ';' }, 2 },
                    { " , ; , a, ; , , b, c, ; , ", new[] { ',', ';' }, 2 },
                    { ";,;a; ;,;b; c;,;", new[] { ',', ';' }, 2 },
                    { " ; , ; a; , ; ; b; c; , ; ", new[] { ',', ';' }, 2 },
                    { "a, b, c", new[] { '_' }, 1 },
                    { "你,,好,, 世, ,\t,\r\n , 界", new[] { ',' }, 2 },
                };
            }
        }

        public static TheoryData<string, char[], StringSegment[]> EnumeratorData
        {
            get
            {
                return new TheoryData<string, char[], StringSegment[]>
                {
                    { string.Empty, new[] { ',' }, Array.Empty<StringSegment>() },
                    { "   ", new[] { ',' }, Array.Empty<StringSegment>() },
                    { ",,,", new[] { ',' }, Array.Empty<StringSegment>() },
                    { "   , , ,  ", new[] { ',' }, Array.Empty<StringSegment>() },
                    { "a,b,c", Array.Empty<char>(), new StringSegment[] { "a,b,c" } },
                    { "a, b, c", Array.Empty<char>(), new StringSegment[] { "a, b, c" } },
                    { ",,,a, ,,,b, c,,,", Array.Empty<char>(), new StringSegment[] { ",,,a, ,,,b, c,,," } },
                    {
                        " , , , a, , , , b, c, , , ",
                        Array.Empty<char>(),
                        new StringSegment[] { ", , , a, , , , b, c, , ," }
                    },
                    {
                        " , , , a, , , , b, c, , , ",
                        new[] { ' ' },
                        new StringSegment[] { ",", ",", ",", "a,", ",", ",", ",", "b,", "c,", ",", "," }
                    },
                    { "a,b,c", new[] { ',' }, new StringSegment[] { "a", "b", "c" } },
                    { " a , b , c ", new[] { ',' }, new StringSegment[] { "a", "b", "c" } },
                    { ",,,a, ,,,b, c,,,", new[] { ',' }, new StringSegment[] { "a", "b", "c" } },
                    { " , , , a, , , , b, c, , , ", new[] { ',' }, new StringSegment[] { "a", "b", "c" } },
                    { ",;,a, ,;,b, c,;,", new[] { ',', ';' }, new StringSegment[] { "a", "b", "c" } },
                    { " , ; , a, ; , , b, c, ; , ", new[] { ',', ';' }, new StringSegment[] { "a", "b", "c" } },
                    { ";,;a; ;,;b; c;,;", new[] { ',', ';' }, new StringSegment[] { "a", "b", "c" } },
                    { " ; , ; a; , ; ; b; c; , ; ", new[] { ',', ';' }, new StringSegment[] { "a", "b", "c" } },
                    { "a, b, c", new[] { '_' }, new StringSegment[] { "a, b, c" } },
                    { "你,,好,, 世, ,\t,\r\n , 界", new[] { ',' }, new StringSegment[] { "你", "好", "世", "界" } },
                };
            }
        }

        public static TheoryData<string, char[], StringSegment[]> EnumeratorMax2Data
        {
            get
            {
                return new TheoryData<string, char[], StringSegment[]>
                {
                    { string.Empty, new[] { ',' }, Array.Empty<StringSegment>() },
                    { "   ", new[] { ',' }, Array.Empty<StringSegment>() },
                    { ",,,", new[] { ',' }, Array.Empty<StringSegment>() },
                    { "   , , ,  ", new[] { ',' }, Array.Empty<StringSegment>() },
                    { "a,b,c", Array.Empty<char>(), new StringSegment[] { "a,b,c" } },
                    { "a, b, c", Array.Empty<char>(), new StringSegment[] { "a, b, c" } },
                    { ",,,a, ,,,b, c,,,", Array.Empty<char>(), new StringSegment[] { ",,,a, ,,,b, c,,," } },
                    {
                        " , , , a, , , , b, c, , , ",
                        Array.Empty<char>(),
                        new StringSegment[] { ", , , a, , , , b, c, , ," }
                    },
                    {
                        " , , , a, , , , b, c, , , ",
                        new[] { ' ' },
                        new StringSegment[] { ",", ", , a, , , , b, c, , ," }
                    },
                    { "a,b,c", new[] { ',' }, new StringSegment[] { "a", "b,c" } },
                    { " a , b , c ", new[] { ',' }, new StringSegment[] { "a", "b , c" } },
                    { ",,,a, ,,,b, c,,,", new[] { ',' }, new StringSegment[] { "a", "b, c,,," } },
                    { " , , , a, , , , b, c, , , ", new[] { ',' }, new StringSegment[] { "a", "b, c, , ," } },
                    { ",;,a, ,;,b, c,;,", new[] { ',', ';' }, new StringSegment[] { "a", "b, c,;," } },
                    { " , ; , a, ; , , b, c, ; , ", new[] { ',', ';' }, new StringSegment[] { "a", "b, c, ; ," } },
                    { ";,;a; ;,;b; c;,;", new[] { ',', ';' }, new StringSegment[] { "a", "b; c;,;" } },
                    { " ; , ; a; , ; ; b; c; , ; ", new[] { ',', ';' }, new StringSegment[] { "a", "b; c; , ;" } },
                    { "a, b, c", new[] { '_' }, new StringSegment[] { "a, b, c" } },
                    {
                        "你,,好,, 世, ,\t,\r\n , 界",
                        new[] { ',' },
                        new StringSegment[] { "你", "好,, 世, ,\t,\r\n , 界" }
                    },
                };
            }
        }

        public static TheoryData<int, int> MaxCountData
        {
            get
            {
                var data = new TheoryData<int, int>();
                for (var i = 0; i < 12; i++)
                {
                    data.Add(i, i);
                }

                data.Add(12, 11);
                data.Add(20, 11);

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(CountData))]
        public void Count_ReturnsExpectedValue(string value, char[] separators, int expected)
        {
            // Arrange
            var tokenizer = new TrimmingTokenizer(value, separators);

            // Act
            var actual = tokenizer.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(CountMax2Data))]
        public void Count_ReturnsExpectedValue_WithMaxCount2(string value, char[] separators, int expected)
        {
            // Arrange
            var tokenizer = new TrimmingTokenizer(value, separators, maxCount: 2);

            // Act
            var actual = tokenizer.Count;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(EnumeratorData))]
        public void SegmentEnumerator_ReturnsExpectedValues(string value, char[] separators, StringSegment[] expected)
        {
            // Arrange
            var segment = (StringSegment)value;
            var tokenizer = new TrimmingTokenizer(segment, separators);

            // Act
            var actual = tokenizer.ToArray();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(EnumeratorData))]
        public void StringEnumerator_ReturnsExpectedValues(string value, char[] separators, StringSegment[] expected)
        {
            // Arrange
            var tokenizer = new TrimmingTokenizer(value, separators);

            // Act
            var actual = tokenizer.ToArray();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(EnumeratorMax2Data))]
        public void SegmentEnumerator_ReturnsExpectedValues_WithMaxCount2(
            string value,
            char[] separators,
            StringSegment[] expected)
        {
            // Arrange
            var segment = (StringSegment)value;
            var tokenizer = new TrimmingTokenizer(segment, separators, maxCount: 2);

            // Act
            var actual = tokenizer.ToArray();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(EnumeratorMax2Data))]
        public void StringEnumerator_ReturnsExpectedValues_WithMaxCount2(
            string value,
            char[] separators,
            StringSegment[] expected)
        {
            // Arrange
            var tokenizer = new TrimmingTokenizer(value, separators, maxCount: 2);

            // Act
            var actual = tokenizer.ToArray();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(MaxCountData))]
        public void Count_ReturnsExpectedValue_WithMaxCount(int maxCount, int expetedCount)
        {
            // Arrange
            var tokenizer = new TrimmingTokenizer(" , , , a, , , , b, c, , , ", new[] { ' ' }, maxCount);

            // Act
            var actual = tokenizer.Count;

            // Assert
            Assert.Equal(expetedCount, actual);
        }
    }
}
