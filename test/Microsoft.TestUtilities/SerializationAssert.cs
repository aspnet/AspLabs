// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.TestUtilities
{
    public static class SerializationAssert
    {
        /// <summary>
        /// Asserts that the serialization matches the expected format. The <paramref name="instance"/> is first verified that all public 
        /// properties have been set so that they are taken into account when verifying the serialization.
        /// </summary>
        /// <typeparam name="T">The type of instance,</typeparam>
        /// <param name="instance">The actual instance to serialize.</param>
        /// <param name="settings">The particular serialization settings to use.</param>
        /// <param name="expected">The expected result of the serialization.</param>
        /// <param name="excludeProperties">An optional collection of public properties that are excluded from the serialization verification.</param>
        public static void SerializesAs<T>(T instance, JsonSerializerSettings settings, string expected, IEnumerable<string> excludeProperties = null)
            where T : class
        {
            // Act
            string actual = JsonConvert.SerializeObject(instance, settings);

            // Act/Assert
            PropertyAssert.PublicPropertiesAreSet(instance, excludeProperties);
            Assert.Equal(expected, actual);
        }
    }
}
