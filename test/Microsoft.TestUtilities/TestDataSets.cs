// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.TestUtilities
{
    public static class TestDataSets
    {
        public static TheoryData<object> MixedInstancesDataSet
        {
            get
            {
                return new TheoryData<object>
                {
                    "test",
                    new string[] { "A", "B", "C" },
                    1,
                    new List<int> { 1, 2, 3 },
                    1.0,
                    Guid.NewGuid(),
                    new Uri("http://localhost")
                };
            }
        }

        public static TheoryData<bool> BoolDataSet
        {
            get
            {
                return new TheoryData<bool> { true, false };
            }
        }

        public static TheoryData<string> EmptyOrWhiteSpaceStringDataSet
        {
            get
            {
                return new TheoryData<string>
                {
                    string.Empty,
                    "   ",
                    "\t",
                    "\u2000",
                    "\u1680",
                    "\u2028",
                    "\u2029",
                };
            }
        }

        public static TheoryData<string, string> CaseInsensitiveDataSet
        {
            get
            {
                return new TheoryData<string, string>
                {
                    { string.Empty, string.Empty },
                    { "test", "TEST" },
                    { "TEST", "test" },
                    { "TeSt", "tEsT" },
                    { "t e s t", "T E S T" },
                    { "你好世界", "你好世界" },
                };
            }
        }
    }
}
