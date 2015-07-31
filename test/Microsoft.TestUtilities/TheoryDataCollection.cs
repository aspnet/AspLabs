// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.TestUtilities
{
    /// <summary>
    /// Provides input data for xUnits' TheoryAttribute.
    /// </summary>
    public abstract class TheoryDataCollection : IEnumerable<object[]>
    {
        private readonly Collection<object[]> _data = new Collection<object[]>();

        public IEnumerator<object[]> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected void AddDataItem(params object[] values)
        {
            _data.Add(values);
        }
    }

    /// <summary>
    /// Provides strongly typed input data for xUnits' TheoryAttribute.
    /// </summary>
    /// <typeparam name="T">Type of the parameter.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "These classes are variants of each other.")]
    public class TheoryDataCollection<T> : TheoryDataCollection
    {
        public void Add(T item)
        {
            AddDataItem(item);
        }
    }

    /// <summary>
    /// Provides strongly typed input data for xUnits' TheoryAttribute.
    /// </summary>
    /// <typeparam name="T1">Type of the first parameter.</typeparam>
    /// <typeparam name="T2">Type of the second parameter.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "These classes are variants of each other.")]
    public class TheoryDataCollection<T1, T2> : TheoryDataCollection
    {
        public void Add(T1 item1, T2 item2)
        {
            AddDataItem(item1, item2);
        }
    }

    /// <summary>
    /// Provides strongly typed input data for xUnits' TheoryAttribute.
    /// </summary>
    /// <typeparam name="T1">Type of the first parameter.</typeparam>
    /// <typeparam name="T2">Type of the second parameter.</typeparam>
    /// <typeparam name="T3">Type of the third parameter.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "These classes are variants of each other.")]
    public class TheoryDataCollection<T1, T2, T3> : TheoryDataCollection
    {
        public void Add(T1 item1, T2 item2, T3 item3)
        {
            AddDataItem(item1, item2, item3);
        }
    }

    /// <summary>
    /// Provides strongly typed input data for xUnits' TheoryAttribute.
    /// </summary>
    /// <typeparam name="T1">Type of the first parameter.</typeparam>
    /// <typeparam name="T2">Type of the second parameter.</typeparam>
    /// <typeparam name="T3">Type of the third parameter.</typeparam>
    /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "These classes are variants of each other.")]
    public class TheoryDataCollection<T1, T2, T3, T4> : TheoryDataCollection
    {
        public void Add(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            AddDataItem(item1, item2, item3, item4);
        }
    }

    /// <summary>
    /// Provides strongly typed input data for xUnits' TheoryAttribute.
    /// </summary>
    /// <typeparam name="T1">Type of the first parameter.</typeparam>
    /// <typeparam name="T2">Type of the second parameter.</typeparam>
    /// <typeparam name="T3">Type of the third parameter.</typeparam>
    /// <typeparam name="T4">Type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">Type of the fifth parameter.</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "These classes are variants of each other.")]
    public class TheoryDataCollection<T1, T2, T3, T4, T5> : TheoryDataCollection
    {
        public void Add(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            AddDataItem(item1, item2, item3, item4, item5);
        }
    }
}
