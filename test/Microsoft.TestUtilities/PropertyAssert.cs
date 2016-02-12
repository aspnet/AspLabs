// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace Microsoft.TestUtilities
{
    public static class PropertyAssert
    {
        private static MethodInfo _isSetMethod = typeof(PropertyAssert).GetMethod("IsSet", BindingFlags.Static | BindingFlags.NonPublic);

        public static PropertyInfo GetPropertyInfo<TInstance, TProperty>(Expression<Func<TInstance, TProperty>> property)
        {
            if (property.Body is MemberExpression)
            {
                return (PropertyInfo)((MemberExpression)property.Body).Member;
            }
            else if (property.Body is UnaryExpression && property.Body.NodeType == ExpressionType.Convert)
            {
                return (PropertyInfo)((MemberExpression)((UnaryExpression)property.Body).Operand).Member;
            }
            else
            {
                throw new InvalidOperationException("Did not find any property to test.");
            }
        }

        public static void Roundtrips<TInstance, TProperty>(TInstance instance, Expression<Func<TInstance, TProperty>> propertyExpression, PropertySetter propertySetter, TProperty defaultValue = null, TProperty roundtripValue = null)
            where TInstance : class
            where TProperty : class
        {
            PropertyInfo property = GetPropertyInfo(propertyExpression);
            Func<TInstance, TProperty> getter = GetPropertyGetter<TInstance, TProperty>(property);
            Action<TInstance, TProperty> setter = GetPropertySetter<TInstance, TProperty>(property);

            switch (propertySetter)
            {
                case PropertySetter.NullRoundtrips:
                    Assert.Equal(defaultValue, getter(instance));
                    TestRoundtrip(instance, getter, setter, roundtripValue: (TProperty)null);
                    break;

                case PropertySetter.NullDoesNotRoundtrip:
                    Assert.NotNull(getter(instance));
                    setter(instance, null);
                    Assert.NotNull(getter(instance));
                    break;

                case PropertySetter.NullSetsDefault:
                    Assert.Equal(defaultValue, getter(instance));
                    TestRoundtrip(instance, getter, setter, roundtripValue: defaultValue);
                    break;

                case PropertySetter.NullThrows:
                    Assert.Equal(defaultValue, getter(instance));
                    TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, null));
                    Assert.IsType<ArgumentNullException>(ex.InnerException);
                    ArgumentNullException argumentNullException = ex.InnerException as ArgumentNullException;
                    Assert.Equal("value", argumentNullException.ParamName);
                    break;

                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Invalid '{0}' value", typeof(PropertySetter).Name));
            }

            if (roundtripValue != null)
            {
                TestRoundtrip(instance, getter, setter, roundtripValue);
            }
        }

        public static void Roundtrips<TInstance, TProperty>(TInstance instance, Expression<Func<TInstance, TProperty?>> propertyExpression, PropertySetter propertySetter, TProperty? defaultValue = null,
            TProperty? minLegalValue = null, TProperty? illegalLowerValue = null,
            TProperty? maxLegalValue = null, TProperty? illegalUpperValue = null,
            TProperty? roundtripValue = null)
            where TInstance : class
            where TProperty : struct
        {
            PropertyInfo property = GetPropertyInfo(propertyExpression);
            Func<TInstance, TProperty?> getter = (obj) => (TProperty?)property.GetValue(obj, index: null);
            Action<TInstance, TProperty?> setter = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(defaultValue, getter(instance));

            switch (propertySetter)
            {
                case PropertySetter.NullRoundtrips:
                    TestRoundtrip(instance, getter, setter, roundtripValue: null);
                    break;

                case PropertySetter.NullSetsDefault:
                    TestRoundtrip(instance, getter, setter, roundtripValue: defaultValue);
                    break;

                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Invalid '{0}' value", typeof(PropertySetter).Name));
            }

            if (roundtripValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, roundtripValue.Value);
            }
            if (minLegalValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, minLegalValue.Value);
            }
            if (maxLegalValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, maxLegalValue.Value);
            }

            if (illegalLowerValue.HasValue)
            {
                TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, illegalLowerValue.Value));
                Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
                ArgumentOutOfRangeException rex = ex.InnerException as ArgumentOutOfRangeException;
                Assert.Equal(illegalLowerValue.Value, rex.ActualValue);
            }

            if (illegalUpperValue.HasValue)
            {
                TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, illegalUpperValue.Value));
                Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
                ArgumentOutOfRangeException rex = ex.InnerException as ArgumentOutOfRangeException;
                Assert.Equal(illegalUpperValue.Value, rex.ActualValue);
            }
        }

        public static void Roundtrips<TInstance, TProperty>(TInstance instance, Expression<Func<TInstance, TProperty>> propertyExpression, TProperty defaultValue = default(TProperty),
            TProperty? minLegalValue = null, TProperty? illegalLowerValue = null,
            TProperty? maxLegalValue = null, TProperty? illegalUpperValue = null,
            TProperty? roundtripValue = null)
            where TInstance : class
            where TProperty : struct
        {
            PropertyInfo property = GetPropertyInfo(propertyExpression);
            Func<TInstance, TProperty> getter = (obj) => (TProperty)property.GetValue(obj, index: null);
            Action<TInstance, TProperty> setter = (obj, value) => property.SetValue(obj, value, index: null);

            Assert.Equal(defaultValue, getter(instance));

            if (roundtripValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, roundtripValue.Value);
            }
            if (minLegalValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, minLegalValue.Value);
            }
            if (maxLegalValue.HasValue)
            {
                TestRoundtrip(instance, getter, setter, maxLegalValue.Value);
            }

            if (illegalLowerValue.HasValue)
            {
                TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, illegalLowerValue.Value));
                Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
                ArgumentOutOfRangeException rex = ex.InnerException as ArgumentOutOfRangeException;
                Assert.Equal(illegalLowerValue.Value, rex.ActualValue);
            }

            if (illegalUpperValue.HasValue)
            {
                TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() => setter(instance, illegalUpperValue.Value));
                Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
                ArgumentOutOfRangeException rex = ex.InnerException as ArgumentOutOfRangeException;
                Assert.Equal(illegalUpperValue.Value, rex.ActualValue);
            }
        }

        /// <summary>
        /// Validates that all public properties have been set on a particular type
        /// and that all public collections have at least one member.
        /// </summary>
        public static void PublicPropertiesAreSet<TInstance>(TInstance instance, IEnumerable<string> excludeProperties = null)
            where TInstance : class
        {
            PropertyInfo[] properties = typeof(TInstance).GetProperties();
            foreach (PropertyInfo p in properties)
            {
                if (excludeProperties != null && excludeProperties.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (p.CanWrite)
                {
                    MethodInfo isSet = _isSetMethod.MakeGenericMethod(p.PropertyType);
                    bool result = (bool)isSet.Invoke(instance, new object[] { p.GetValue(instance) });
                    Assert.True(result, string.Format("Parameter '{0}' was not set on type '{1}'", p.Name, typeof(TInstance).Name));
                }
                else if (typeof(IEnumerable).IsAssignableFrom(p.PropertyType))
                {
                    Assert.NotEmpty((IEnumerable)p.GetValue(instance));
                }
            }
        }

        private static bool IsSet<T>(T value)
        {
            return !EqualityComparer<T>.Default.Equals(value, default(T));
        }

        private static Func<TInstance, TProperty> GetPropertyGetter<TInstance, TProperty>(PropertyInfo property)
        {
            return (instance) => (TProperty)property.GetValue(instance, index: null);
        }

        private static Action<TInstance, TProperty> GetPropertySetter<TInstance, TProperty>(PropertyInfo property)
        {
            return (instance, value) => property.SetValue(instance, value, index: null);
        }

        private static void TestRoundtrip<TInstance, TProperty>(TInstance instance, Func<TInstance, TProperty> getter, Action<TInstance, TProperty> setter, TProperty roundtripValue)
            where TInstance : class
        {
            setter(instance, roundtripValue);
            TProperty actual = getter(instance);
            Assert.Equal(roundtripValue, actual);
        }
    }
}
