// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Adapters;

public abstract class DelegatingSessionState : ISessionState
{
    protected DelegatingSessionState()
    {
    }

    protected abstract ISessionState State { get; }

    public virtual object? this[string name]
    {
        get => State[name];
        set => State[name] = value;
    }

    public virtual string SessionID => State.SessionID;

    public virtual int Count => State.Count;

    public virtual bool IsReadOnly => State.IsReadOnly;

    public virtual int Timeout { get => State.Timeout; set => State.Timeout = value; }

    public virtual bool IsNewSession => State.IsNewSession;

    public virtual bool IsSynchronized => State.IsSynchronized;

    public virtual object SyncRoot => State.SyncRoot;

    public virtual void Abandon() => State.Abandon();

    public virtual void Add(string name, object value) => State.Add(name, value);

    public virtual void Clear() => State.Clear();

    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            State.Dispose();
        }
    }

    public virtual void Remove(string name) => State.Remove(name);

    public virtual ValueTask CommitAsync(CancellationToken token) => State.CommitAsync(token);

    public virtual void CopyTo(Array array, int index) => State.CopyTo(array, index);

    public virtual IEnumerator GetEnumerator() => State.GetEnumerator();
}
