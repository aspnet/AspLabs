// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.WebHooks.Utilities
{
    /// <summary>
    /// Provides various <see cref="Type"/> related utilities.
    /// </summary>
    public static class TypeUtilities
    {
        /// <summary>
        /// Checks whether <paramref name="type"/> is a visible, non-abstract class of type <typeparamref name="T"/> or 
        /// derived from type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to test against.</typeparam>
        /// <param name="type">The type to test.</param>
        /// <returns><c>true</c> if the type is of type <typeparamref name="T"/>.</returns>
        public static bool IsType<T>(Type type)
        {
            return
                type != null &&
                type.IsClass &&
                type.IsVisible &&
                !type.IsAbstract &&
                typeof(T).IsAssignableFrom(type);
        }

        /// <summary>
        /// Finds types matching the <paramref name="predicate"/> in a given set of <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">The assemblies to look through.</param>
        /// <param name="predicate">The predicate to apply to the search.</param>
        /// <returns>An <see cref="ICollection{T}"/> of types found.</returns>
        public static ICollection<Type> GetTypes(IEnumerable<Assembly> assemblies, Func<Type, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            List<Type> result = new List<Type>();
            if (assemblies == null)
            {
                return result;
            }

            // Go through all assemblies and search for types matching the predicate
            foreach (Assembly assembly in assemblies)
            {
                Type[] exportedTypes = null;
                if (assembly == null)
                {
                    continue;
                }

                if (assembly.IsDynamic)
                {
                    // Can't call GetExportedTypes on a dynamic assembly
                    continue;
                }

                try
                {
                    exportedTypes = assembly.GetExportedTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    // We deliberately ignore all other exceptions. 
                    continue;
                }

                if (exportedTypes != null)
                {
                    result.AddRange(exportedTypes.Where(x => predicate(x)));
                }
            }
            return result;
        }

        /// <summary>
        /// Finds types matching the <paramref name="predicate"/> in a given set of <paramref name="assemblies"/> 
        /// and creates instances of those type using the default constructor.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the instances to create.</typeparam>
        /// <param name="assemblies">The assemblies to look through.</param>
        /// <param name="predicate">The predicate to apply to the search.</param>
        /// <returns>An <see cref="ICollection{T}"/> of instances found.</returns>
        public static ICollection<T> GetInstances<T>(IEnumerable<Assembly> assemblies, Func<Type, bool> predicate)
        {
            ICollection<Type> types = GetTypes(assemblies, predicate);

            // Create instances using default public constructor
            List<T> instances = new List<T>();
            foreach (Type t in types)
            {
                instances.Add((T)Activator.CreateInstance(t));
            }
            return instances;
        }
    }
}
