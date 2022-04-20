// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using System.Web.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class SessionSerializer
{
    public byte[] Serialize(HttpSessionState state)
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

        return JsonSerializer.SerializeToUtf8Bytes(session, _options);
    }

    public void DeserializeTo(ReadOnlySpan<byte> data, HttpSessionState state)
    {
        var result = JsonSerializer.Deserialize<SerializedSessionState>(data, _options);

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
