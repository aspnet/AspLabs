// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Web.SessionState;

namespace System.Web.Adapters;

/// <summary>
/// Represents the state of a session and is used to create a <see cref="HttpSessionState"/>.
/// </summary>
public interface ISessionState
{
    string SessionID { get; }

    int Count { get; }

    bool IsReadOnly { get; }

    int Timeout { get; }

    bool IsNewSession { get; }

    object? this[string name] { get; set; }

    IEnumerable<string> Keys { get; }
}
