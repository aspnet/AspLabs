// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class ParameterCollectionTests
    {
        public static TheoryData<IDictionary<string, string>, string> CollectionData
        {
            get
            {
                return new TheoryData<IDictionary<string, string>, string>
                {
                    { GetParameters(0), string.Empty },
                    { GetParameters(1), "p0=v0" },
                    { GetParameters(2), "p0=v0; p1=v1" },
                    { GetParameters(3), "p0=v0; p1=v1; p2=v2" },
                };
            }
        }

        [Theory]
        [MemberData(nameof(CollectionData))]
        public void ToString_PrintsCorrectly(IDictionary<string, string> parameters, string expected)
        {
            // Arrange
            ParameterCollection collection = new ParameterCollection();
            foreach (var parameter in parameters)
            {
                collection.Add(parameter.Key, parameter.Value);
            }

            // Act
            string actual = collection.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }

        private static IDictionary<string, string> GetParameters(int count)
        {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            for (int cnt = 0; cnt < count; cnt++)
            {
                parameters.Add("p" + cnt, "v" + cnt);
            }
            return parameters;
        }
    }
}
