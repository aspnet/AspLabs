// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;

namespace System.Web
{
    public sealed class HttpCookieCollection : NameObjectCollectionBase
    {
        public string[] AllKeys { get => throw new NotImplementedException(); }

        public HttpCookie this[string name] { get => throw new NotImplementedException(); }

        public HttpCookie this[int index] { get => throw new NotImplementedException(); }

        public void Add(HttpCookie cookie) => throw new NotImplementedException();

        public void Set(HttpCookie cookie) => throw new NotImplementedException();

        public HttpCookie Get(string name) => this[name];

        public HttpCookie Get(int index) => this[index];

        public string GetKey(int index) => this[index].Name;

        public void Remove(string name) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();
    }
}
