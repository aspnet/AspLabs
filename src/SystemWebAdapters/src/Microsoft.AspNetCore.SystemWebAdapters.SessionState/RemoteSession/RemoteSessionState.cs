// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal class RemoteSessionState : DelegatingSessionState
{
    private HttpResponseMessage? _response;
    private Func<ISessionState?, CancellationToken, Task>? _commitOrRelease;

    public RemoteSessionState(ISessionState other, HttpResponseMessage response, Func<ISessionState?, CancellationToken, Task> commitOrRelease)
    {
        State = other;
        _response = response;
        _commitOrRelease = commitOrRelease;
    }

    protected override ISessionState State { get; }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_response is not null)
        {
            _response.Dispose();
            _response = null;
        }

        await base.DisposeAsyncCore();
    }

    public override async ValueTask CommitAsync(CancellationToken token)
    {
        if (_commitOrRelease is { } onCommit)
        {
            _commitOrRelease = null;
            await onCommit(State, token);
        }
    }
}

