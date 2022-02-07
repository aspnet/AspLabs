// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace System.Web
{
    public class VerifyTypeForwards
    {
        private readonly ITestOutputHelper _output;

        public VerifyTypeForwards(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void VerifyPublicTypesAreForwardedToSystemWeb()
        {
            var exportedTypes = typeof(HttpContextBase).Assembly
                .GetExportedTypes()
                .Select(t => t.FullName)
                .ToHashSet();

            WriteExpectedFile(exportedTypes);

            // All the exported types (and only them) should be forwarded
            Assert.True(exportedTypes.SetEquals(GetForwardedTypes()));
        }

        private static IEnumerable<string> GetForwardedTypes()
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "adapters", "netfx");
            using var frameworkContext = new MetadataLoadContext(new PathAssemblyResolver(Directory.EnumerateFiles(dir, "*.dll")));

            return frameworkContext.LoadFromAssemblyName("System.Web.Adapters")
                .GetForwardedTypes()
                .Select(t => t.FullName)
                .ToList();
        }

        private void WriteExpectedFile(IEnumerable<string> types)
        {
            _output.WriteLine("// Ensure the following type forwards are present in the .NET Framework compilation of the adapters");
            _output.WriteLine(string.Empty);
            _output.WriteLine("using System.Runtime.CompilerServices;");
            _output.WriteLine(string.Empty);

            foreach (var type in types)
            {
                _output.WriteLine($"[assembly: TypeForwardedTo(typeof(global::{type}))]");
            }
        }
    }
}
