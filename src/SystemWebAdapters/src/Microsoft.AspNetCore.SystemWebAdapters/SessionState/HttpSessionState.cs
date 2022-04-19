// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.AspNetCore.SystemWebAdapters;

namespace System.Web.SessionState;

public class HttpSessionState : ICollection
{
    private readonly ISessionState _container;

    public HttpSessionState(ISessionState container)
    {
        _container = container;
    }

    public string SessionID => _container.SessionID;

    public int Count => _container.Count;

    public bool IsReadOnly => _container.IsReadOnly;

    public bool IsNewSession { get; }

    public int Timeout
    {
        get => _container.Timeout;
        set => _container.Timeout = value;
    }

    public bool IsSynchronized => _container.IsSynchronized;

    public object SyncRoot => _container.SyncRoot;

    public void Abandon() => _container.Abandon();

    public object? this[string name]
    {
        get => _container[name];
        set => _container[name] = value;
    }

    public void Add(string name, object value) => _container.Add(name, value);

    public void Remove(string name) => _container.Remove(name);

    public void RemoveAll() => _container.Clear();

    public void Clear() => _container.Clear();

    public void CopyTo(Array array, int index) => _container.CopyTo(array, index);

    public IEnumerator GetEnumerator() => _container.Keys.GetEnumerator();
}
