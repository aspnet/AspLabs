// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Reflection;
using Type = System.Type;

namespace Microsoft.AspNetCore.Grpc.HttpApi.Tests.Converter
{
    // Effectively a cache of mapping from enum values to the original name as specified in the proto file,
    // fetched by reflection.
    // The need for this is unfortunate, as is its unbounded size, but realistically it shouldn't cause issues.
    internal static class OriginalEnumValueHelper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<object, string>> _dictionaries
            = new ConcurrentDictionary<Type, Dictionary<object, string>>();

        internal static string? GetOriginalName(object value)
        {
            var enumType = value.GetType();
            Dictionary<object, string>? nameMapping;
            lock (_dictionaries)
            {
                if (!_dictionaries.TryGetValue(enumType, out nameMapping))
                {
                    nameMapping = GetNameMapping(enumType);
                    _dictionaries[enumType] = nameMapping;
                }
            }

            string? originalName;
            // If this returns false, originalName will be null, which is what we want.
            nameMapping.TryGetValue(value, out originalName);
            return originalName;
        }

        private static Dictionary<object, string> GetNameMapping(Type enumType)
        {
            return enumType.GetTypeInfo().DeclaredFields
                .Where(f => f.IsStatic)
                .Where(f => f.GetCustomAttributes<OriginalNameAttribute>()
                             .FirstOrDefault()?.PreferredAlias ?? true)
                .ToDictionary(f => f.GetValue(null)!,
                              f => f.GetCustomAttributes<OriginalNameAttribute>()
                                    .FirstOrDefault()
                                    // If the attribute hasn't been applied, fall back to the name of the field.
                                    ?.Name ?? f.Name);
        }
    }
}
