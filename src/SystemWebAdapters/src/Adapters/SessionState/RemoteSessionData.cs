// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters.SessionState;

internal class RemoteSessionData
{
    public bool Abandon { get; set; }

    public string SessionID { get; set; } = null!;

    public bool IsReadOnly { get; set; }

    public SessionValues Values { get; set; } = null!;

    public int Timeout { get; set; }

    public bool IsNewSession { get; set; }
}
