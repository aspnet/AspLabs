// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.FunctionalTest
{
    /// <summary>
    /// Reader for files compiled into an assembly as resources.
    /// </summary>
    /// <remarks>
    /// A simplified version of aspnet/Mvc's ResourceFile class. Lacks the ability to create or update files i.e. does
    /// not include the original's GENERATE_BASELINES support. In turn, the MVC class was inspired by aspnet/Razor's
    /// BaselineWriter and TestFile test classes.
    /// </remarks>
    public class ResourceFile
    {
        private static readonly Assembly _resourcesAssembly = typeof(ResourceFile).GetTypeInfo().Assembly;

        /// <summary>
        /// Return <see cref="Stream"/> for <paramref name="resourceName"/> from the <see cref="Assembly"/>'s manifest.
        /// The <see cref="Assembly"/> used is the one containing <see cref="ResourceFile"/> and the test classes.
        /// </summary>
        /// <param name="resourceName">
        /// Name of the manifest resource in the <see cref="Assembly"/>. Also, a path relative to the test project's
        /// directory.
        /// </param>
        /// <param name="normalizeLineEndings">
        /// If <c>true</c> <paramref name="resourceName"/> is used as a source file and its line endings must be
        /// normalized.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <see cref="Stream"/> for <paramref name="resourceName"/>
        /// from the <see cref="Assembly"/>'s manifest.
        /// </returns>
        /// <exception cref="Xunit.Sdk.TrueException">
        /// Thrown if <paramref name="resourceName"/> is not found in the <see cref="Assembly"/>.
        /// </exception>
        /// <remarks>
        /// Normalizes line endings to "\r\n" (CRLF) if <paramref name="normalizeLineEndings"/> is
        /// <see langword="true"/>.
        /// </remarks>
        public static async Task<Stream> GetResourceStreamAsync(string resourceName, bool normalizeLineEndings)
        {
            var dottedResourceName = resourceName.Replace('/', '.').Replace('\\', '.');
            var fullName = $"{ _resourcesAssembly.GetName().Name }.{ dottedResourceName }";
            Assert.True(Exists(fullName), $"Manifest resource '{ fullName }' not found.");

            var stream = _resourcesAssembly.GetManifestResourceStream(fullName);
            if (normalizeLineEndings)
            {
                // Normalize line endings to '\r\n' (CRLF). This removes core.autocrlf, core.eol, core.safecrlf, and
                // .gitattributes from the equation and treats "\r\n" and "\n" as equivalent. Does not handle
                // some line endings like "\r" but otherwise ensures checksums and line mappings are consistent.
                string text;
                using (var streamReader = new StreamReader(stream))
                {
                    var content = await streamReader.ReadToEndAsync();
                    text = content.Replace("\r", "").Replace("\n", "\r\n");
                }

                var bytes = Encoding.UTF8.GetBytes(text);
                stream = new MemoryStream(bytes);
            }

            return stream;
        }

        /// <summary>
        /// Return <see cref="string"/> content of <paramref name="resourceName"/> from the <see cref="Assembly"/>'s
        /// manifest. The <see cref="Assembly"/> used is the one containing <see cref="ResourceFile"/> and the test
        /// classes.
        /// </summary>
        /// <param name="resourceName">
        /// Name of the manifest resource in the <see cref="Assembly"/>. Also, a path relative to the test project's
        /// directory.
        /// </param>
        /// <param name="normalizeLineEndings">
        /// If <c>true</c> <paramref name="resourceName"/> is used as a source file and its line endings must be
        /// normalized.
        /// </param>
        /// <returns>
        /// A <see cref="Task{string}"/> that on completion returns the <see cref="string"/> content of
        /// <paramref name="resourceName"/> from the <see cref="Assembly"/>'s manifest.
        /// </returns>
        /// <exception cref="Xunit.Sdk.TrueException">
        /// Thrown if <paramref name="resourceName"/> is not found in the <see cref="Assembly"/>.
        /// </exception>
        /// <remarks>
        /// Normalizes line endings to "\r\n" (CRLF) if <paramref name="normalizeLineEndings"/> is
        /// <see langword="true"/>.
        /// </remarks>
        public static async Task<string> GetResourceAsStringAsync(string resourceName, bool normalizeLineEndings)
        {
            using (var stream = await GetResourceStreamAsync(resourceName, normalizeLineEndings))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var streamReader = new StreamReader(stream))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        private static bool Exists(string fullName)
        {
            var resourceNames = _resourcesAssembly.GetManifestResourceNames();
            foreach (var resourceName in resourceNames)
            {
                // Resource names are case-sensitive.
                if (string.Equals(fullName, resourceName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
