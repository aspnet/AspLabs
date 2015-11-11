// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.WebHooks.Controllers;
using Xunit;

namespace Microsoft.AspNet.WebHooks.WebHooks
{
    public class WebHookAssemblyResolverTests
    {
        private readonly WebHookAssemblyResolver _resolver;

        public WebHookAssemblyResolverTests()
        {
            _resolver = new WebHookAssemblyResolver();
        }

        [Fact]
        public void GetAssemblies_EnsuresReceiverAssemblyIsAdded()
        {
            // Act
            ICollection<Assembly> actual = _resolver.GetAssemblies();

            // Assert
            Assembly expected = typeof(WebHookReceiversController).Assembly;
            IEnumerable<Assembly> found = actual.Where(item => item == expected);
            Assert.Equal(1, found.Count());
        }

        [Fact]
        public void GetAssemblies_IsIdempotent()
        {
            // Act
            ICollection<Assembly> actual1 = _resolver.GetAssemblies();
            ICollection<Assembly> actual2 = _resolver.GetAssemblies();

            // Assert
            Assert.Equal(actual1, actual2);
        }
    }
}
