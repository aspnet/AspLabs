// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Specialized;

namespace System.Web.SessionState
{
    public class HttpSessionState : ICollection
    {
        public HttpSessionStateBase Contents => throw new NotImplementedException();

        public NameObjectCollectionBase.KeysCollection Keys => throw new NotImplementedException();

        public string SessionID => throw new NotImplementedException();

        public int Timeout
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public object this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public object this[string name]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public void Abandon() => throw new NotImplementedException();

        public void Add(string name, object value) => throw new NotImplementedException();

        public void Clear() => throw new NotImplementedException();

        public void Remove(string name) => throw new NotImplementedException();

        public void RemoveAll() => throw new NotImplementedException();

        public void RemoveAt(int index) => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public void CopyTo(Array array, int index) => throw new NotImplementedException();

        public IEnumerator GetEnumerator() => throw new NotImplementedException();
    }
}
