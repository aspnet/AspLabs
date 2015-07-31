// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Extensions
{
    public class CollectionExtensionsTests
    {
        [Fact]
        public void AddRange_AddsRange()
        {
            // Arrange
            IEnumerable<object> additions = new[] { new object(), new object() };
            Collection<object> collection = new Collection<object>() { new object(), new object() };
            Collection<object> expected = new Collection<object>(collection);
            foreach (var item in additions)
            {
                expected.Add(item);
            }

            // Act
            collection.AddRange(additions);

            // Assert
            Assert.Equal(expected, collection);
        }
    }
}
