// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Web.SessionState;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

public interface ISessionSerializer
{
    void DeserializeTo(ReadOnlySpan<byte> data, HttpSessionState state);

    byte[] Serialize(HttpSessionState state);
}
