// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.WebHooks.Utilities
{
    /// <summary>
    /// Utility methods for extracting <see cref="StringValues"/> from objects using JSON paths and XPaths.
    /// </summary>
    internal static class ObjectPathUtilities
    {
        /// <summary>
        /// Gets the <see cref="StringValues"/> that match <paramref name="jsonPath"/> in the <paramref name="json"/>.
        /// </summary>
        /// <param name="json">The <see cref="JContainer"/> to search.</param>
        /// <param name="jsonPath">The JSON path to match.</param>
        /// <returns>
        /// The <see cref="StringValues"/> that match <paramref name="jsonPath"/> in the <paramref name="json"/>.
        /// </returns>
        public static StringValues GetStringValues(JContainer json, string jsonPath)
        {
            var tokens = json.SelectTokens(jsonPath);
            var count = tokens.Count();
            switch (count)
            {
                case 0:
                    return StringValues.Empty;

                case 1:
                    return new StringValues((string)tokens.First());

                default:
                    var eventArray = new string[count];
                    var i = 0;
                    foreach (var token in tokens)
                    {
                        eventArray[i++] = (string)token;
                    }

                    return new StringValues(eventArray);
            }
        }

        /// <summary>
        /// Gets the <see cref="StringValues"/> that match <paramref name="xPath"/> in the <paramref name="xml"/>.
        /// </summary>
        /// <param name="xml">The <see cref="XElement"/> to search.</param>
        /// <param name="xPath">The XPath to match.</param>
        /// <returns>
        /// The <see cref="StringValues"/> that match <paramref name="xPath"/> in the <paramref name="xml"/>.
        /// </returns>
        public static StringValues GetStringValues(XElement xml, string xPath)
        {
            var elements = xml.XPathSelectElements(xPath);
            var count = elements.Count();
            switch (count)
            {
                case 0:
                    return StringValues.Empty;

                case 1:
                    return new StringValues(elements.First().Value);

                default:
                    var eventArray = new string[count];
                    var i = 0;
                    foreach (var element in elements)
                    {
                        eventArray[i++] = element.Value;
                    }

                    return new StringValues(eventArray);
            }
        }
    }
}
