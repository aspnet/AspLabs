// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.SessionState;

namespace System.Web;

public class HttpSessionStateWrapper : HttpSessionStateBase
{
    private readonly HttpSessionState _session;

    public HttpSessionStateWrapper(HttpSessionState session)
    {
        _session = session;
    }

    public override string SessionID => _session.SessionID;

    public override int Count => _session.Count;

    public override bool IsReadOnly => _session.IsReadOnly;

    public override bool IsNewSession => _session.IsNewSession;

    public override int TimeOut
    {
        get => _session.TimeOut;
        set => _session.TimeOut = value;
    }

    public override void Abandon() => _session.Abandon();

    public override object? this[string name]
    {
        get => _session[name];
        set => _session[name] = value;
    }

    public override void Add(string name, object value) => _session.Add(name, value);

    public override void Remove(string name) => _session.Remove(name);

    public override void RemoveAll() => _session.RemoveAll();

    public override void Clear() => _session.Clear();
}
