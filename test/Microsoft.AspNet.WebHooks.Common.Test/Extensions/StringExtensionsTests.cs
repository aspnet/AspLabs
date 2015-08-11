// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace System
{
    public class StringExtensionsTests
    {
        public static TheoryData<string, char[], string[]> SplitAndTrimData
        {
            get
            {
                return new TheoryData<string, char[], string[]>
                {
                    { null, new[] { ',' }, new string[0] },
                    { string.Empty, new[] { ',' }, new string[0] },
                    { "   ", new[] { ',' }, new string[0] },
                    { "a,b,c", null, new string[] { "a,b,c" } },
                    { "a, b, c", null, new string[] { "a,", "b,", "c" } },
                    { "a,b,c", new char[0], new string[] { "a,b,c" } },
                    { "a, b, c", new char[0], new string[] { "a,", "b,", "c" } },
                    { "a,b,c", new[] { ',' }, new string[] { "a", "b", "c" } },
                    { " a , b , c ", new[] { ',' }, new string[] { "a", "b", "c" } },
                    { "a, b, c", new[] { '_' }, new string[] { "a, b, c" } },
                    { "你,,好,, 世, ,\t,\r\n , 界", new[] { ',' }, new string[] { "你", "好", "世", "界" } },
                };
            }
        }

        [Theory]
        [MemberData("SplitAndTrimData")]
        public void SplitAndTrim_ReturnsExpectedResult(string input, char[] split, string[] expected)
        {
            // Act
            IEnumerable<string> actual = input.SplitAndTrim(split);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
