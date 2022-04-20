// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal partial class SerializedSessionState
#if NETCOREAPP3_1_OR_GREATER
    : ISessionState
#endif
{
    public object? this[string name]
    {
        get => Values[name];
        set => Values[name] = value;
    }

    [JsonPropertyName("id")]
    public string SessionID { get; set; } = null!;

    [JsonPropertyName("r")]
    public bool IsReadOnly { get; set; }

    [JsonIgnore]
    public SessionValues Values => RawValues ??= new();

    [JsonPropertyName("v")]
    public SessionValues? RawValues { get; set; }

    [JsonPropertyName("t")]
    public int Timeout { get; set; }

    [JsonPropertyName("n")]
    public bool IsNewSession { get; set; }

    [JsonPropertyName("a")]
    public bool IsAbandoned { get; set; }

    [JsonIgnore]
    public int Count => RawValues?.Count ?? 0;

#if NETCOREAPP3_1_OR_GREATER
    bool ISessionState.IsSynchronized => ((ICollection)Values).IsSynchronized;

    object ISessionState.SyncRoot => ((ICollection)Values).SyncRoot;

    void ISessionState.Add(string name, object value) => Values.Add(name, value);

    void ISessionState.Clear() => RawValues?.Clear();

    void ISessionState.Remove(string name) => RawValues?.Remove(name);

    ValueTask ISessionState.CommitAsync(CancellationToken token) => default;

    IEnumerable<string> ISessionState.Keys => RawValues?.Keys ?? Enumerable.Empty<string>();

    ValueTask IAsyncDisposable.DisposeAsync() => default;
#endif
}
