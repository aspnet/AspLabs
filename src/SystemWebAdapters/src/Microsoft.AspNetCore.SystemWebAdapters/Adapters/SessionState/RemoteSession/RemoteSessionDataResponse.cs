// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

internal class RemoteSessionDataResponse: IDisposable
{
    public RemoteSessionData RemoteSessionData { get; }
    public HttpResponseMessage HttpRespone { get; }

    public RemoteSessionDataResponse(RemoteSessionData remoteSessionData, HttpResponseMessage httpResponse)
    {
        RemoteSessionData = remoteSessionData;
        HttpRespone = httpResponse;
    }

    public void Dispose() => HttpRespone.Dispose();
}
