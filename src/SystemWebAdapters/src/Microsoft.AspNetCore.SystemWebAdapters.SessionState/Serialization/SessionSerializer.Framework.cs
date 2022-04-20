// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class SessionSerializer
{
    public Task SerializeAsync(HttpSessionState state, Stream stream, CancellationToken token)
    {
        if (state is null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        var session = new SerializedSessionState
        {
            IsNewSession = state.IsNewSession,
            IsReadOnly = state.IsReadOnly,
            SessionID = state.SessionID,
            Timeout = state.Timeout,
        };

        foreach (string key in state.Keys)
        {
            session.Values.Add(key, state[key]);
        }

        return JsonSerializer.SerializeAsync(stream, session, _options, token);
    }

    public async Task DeserializeToAsync(Stream stream, HttpSessionState state, CancellationToken token)
    {
        var result = await JsonSerializer.DeserializeAsync<SerializedSessionState>(stream, _options, token);

        if (result is null)
        {
            return;
        }

        if (!string.Equals(state.SessionID, result.SessionID, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Session id must match");
        }

        if (result.IsAbandoned)
        {
            state.Abandon();
            return;
        }

        state.Timeout = result.Timeout;
        state.Clear();

        foreach (var key in result.Values.Keys)
        {
            state[key] = result[key];
        }
    }
}
