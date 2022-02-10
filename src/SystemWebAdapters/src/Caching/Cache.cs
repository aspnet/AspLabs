// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Caching
{
    public class Cache
    {
        public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;

        public static readonly TimeSpan NoSlidingExpiration = TimeSpan.Zero;

        public object this[string key]
        {
            get => Get(key);
            set => Insert(key, value);
        }

        public object Get(string key) => throw new NotImplementedException();

        public void Insert(string key, object value) => throw new NotImplementedException();

        public object Remove(string key) => throw new NotImplementedException();
    }
}
