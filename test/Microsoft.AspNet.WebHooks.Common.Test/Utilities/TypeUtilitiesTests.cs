// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Utilities
{
    public class TypeUtilitiesTests
    {
        public interface ITestType
        {
        }

        public interface INotImplementedTestType
        {
        }

        public static TheoryData<Type, Type, bool> IsTypeData
        {
            get
            {
                return new TheoryData<Type, Type, bool>
                {
                    { DateTime.Now.GetType(), typeof(DateTime), false },
                    { DayOfWeek.Saturday.GetType(), typeof(int), false },
                    { typeof(List<int>), typeof(List<string>), false },
                    { typeof(KeyedCollection<string, string>), typeof(TestKeyedCollection), true },
                    { typeof(ITestType), typeof(TestType), true },
                    { "Hello".GetType(), typeof(string), true },
                    { typeof(List<string>), typeof(List<string>), true },
                };
            }
        }

        [Theory]
        [MemberData(nameof(IsTypeData))]
        public void IsType_DetectsTypes(Type testType, Type type, bool expected)
        {
            // Arrange
            var genericIsTypeMethod = typeof(TypeUtilities).GetMethod("IsType");
            var isTypeMethod = genericIsTypeMethod.MakeGenericMethod(testType);

            // Act
            var actual = (bool)isTypeMethod.Invoke(null, new[] { type });

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetTypes_ReturnsExpectedTypes()
        {
            // Arrange
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Act
            var actual = TypeUtilities.GetTypes(assemblies, t => TypeUtilities.IsType<ITestType>(t));

            // Assert
            Assert.Equal(1, actual.Count);
            Assert.Equal(typeof(TestType), actual.Single());
        }

        [Fact]
        public void GetTypes_ReturnsEmptyIfNoneFound()
        {
            // Arrange
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Act
            var actual = TypeUtilities.GetTypes(assemblies, t => TypeUtilities.IsType<INotImplementedTestType>(t));

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetTypes_ReturnsEmptyListIfNullAssemblies()
        {
            // Act
            var actual = TypeUtilities.GetTypes(null, t => TypeUtilities.IsType<INotImplementedTestType>(t));

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetTypes_ReturnsEmptyListIfNullAssemblyEntries()
        {
            // Arrange
            var assemblies = new Assembly[4];

            // Act
            var actual = TypeUtilities.GetTypes(assemblies, t => TypeUtilities.IsType<INotImplementedTestType>(t));

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetInstances_CreatesExpectedInstances()
        {
            // Arrange
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Act
            var actual = TypeUtilities.GetInstances<ITestType>(assemblies, t => TypeUtilities.IsType<ITestType>(t));

            // Assert
            Assert.Equal(1, actual.Count);
            Assert.IsType<TestType>(actual.Single());
        }

        public class TestType : ITestType
        {
        }

        public class TestKeyedCollection : KeyedCollection<string, string>
        {
            protected override string GetKeyForItem(string item)
            {
                throw new NotImplementedException();
            }
        }

        internal class NotVisibleTest
        {
        }
    }
}
