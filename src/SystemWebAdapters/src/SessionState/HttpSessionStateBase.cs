// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public abstract class HttpSessionStateBase
{
    public virtual string SessionID => throw new NotImplementedException();

    public virtual int Count => throw new NotImplementedException();

    public virtual bool IsReadOnly => throw new NotImplementedException();

    public virtual int TimeOut
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public virtual bool IsNewSession => throw new NotImplementedException();

    public virtual void Abandon() => throw new NotImplementedException();

    public virtual object? this[string name]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public virtual void Add(string name, object value) => throw new NotImplementedException();

    public virtual void Remove(string name) => throw new NotImplementedException();

    public virtual void RemoveAll() => throw new NotImplementedException();

    public virtual void Clear() => throw new NotImplementedException();
}
