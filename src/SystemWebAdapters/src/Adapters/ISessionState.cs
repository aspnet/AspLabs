// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace System.Web.Adapters;

/// <summary>
/// Represents the state of a session and is used to create a <see cref="HttpSessionState"/>.
/// </summary>
public interface ISessionState : IDictionary<string, object?>, IDisposable
{
    string SessionID { get; }

    int Timeout { get; set; }

    bool IsNewSession { get; }

    void Abandon();

    Task CommitAsync(HttpContextCore context, CancellationToken cancellationToken = default);
}
