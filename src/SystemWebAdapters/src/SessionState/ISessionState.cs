// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.SessionState;

public interface ISessionState
{
    string SessionID { get; }

    int Count { get; }

    bool IsReadOnly { get; }

    int TimeOut { get; set; }

    bool IsNewSession { get; }

    void Abandon();

    object this[string name] { get; set; }

    void Add(string name, object value);

    void Remove(string name);

    void Clear();
}
