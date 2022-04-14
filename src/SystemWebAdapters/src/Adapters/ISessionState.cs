// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace System.Web.Adapters;

/// <summary>
/// Represents the state of a session and is used to create a <see cref="HttpSessionState"/> . Disposing the state will handle any writing that may need to be done.
/// </summary>
public interface ISessionState : IDisposable
{
    string SessionID { get; }

    bool IsReadOnly { get; }

    int Timeout { get; set; }

    bool IsNewSession { get; }

    int Count { get; }

    bool IsSynchronized { get; }

    object SyncRoot { get; }

    void Abandon();

    object? this[string name] { get; set; }

    void Add(string name, object value);

    void CopyTo(Array array, int index);

    void Remove(string name);

    void Clear();

    IEnumerator GetEnumerator();

    ValueTask CommitAsync(CancellationToken token);
}
