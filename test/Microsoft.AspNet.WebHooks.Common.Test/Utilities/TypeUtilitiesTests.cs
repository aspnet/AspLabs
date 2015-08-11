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
        [MemberData("IsTypeData")]
        public void IsType_DetectsTypes(Type testType, Type type, bool expected)
        {
            // Arrange
            MethodInfo genericIsTypeMethod = typeof(TypeUtilities).GetMethod("IsType");
            MethodInfo isTypeMethod = genericIsTypeMethod.MakeGenericMethod(testType);

            // Act
            bool actual = (bool)isTypeMethod.Invoke(null, new[] { type });

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetTypes_ReturnsExpectedTypes()
        {
            // Arrange
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            // Act
            ICollection<Type> actual = TypeUtilities.GetTypes(asms, t => TypeUtilities.IsType<ITestType>(t));

            // Assert
            Assert.Equal(1, actual.Count);
            Assert.Equal(typeof(TestType), actual.Single());
        }

        [Fact]
        public void GetTypes_ReturnsEmptyIfNoneFound()
        {
            // Arrange
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            // Act
            ICollection<Type> actual = TypeUtilities.GetTypes(asms, t => TypeUtilities.IsType<INotImplementedTestType>(t));

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetTypes_ReturnsEmptyListIfNullAssemblies()
        {
            // Act
            ICollection<Type> actual = TypeUtilities.GetTypes(null, t => TypeUtilities.IsType<INotImplementedTestType>(t));

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetTypes_ReturnsEmptyListIfNullAssemblyEntries()
        {
            // Arrange
            Assembly[] asms = new Assembly[4];

            // Act
            ICollection<Type> actual = TypeUtilities.GetTypes(asms, t => TypeUtilities.IsType<INotImplementedTestType>(t));

            // Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetInstances_CreatesExpectedInstances()
        {
            // Arrange
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

            // Act
            ICollection<ITestType> actual = TypeUtilities.GetInstances<ITestType>(asms, t => TypeUtilities.IsType<ITestType>(t));

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
