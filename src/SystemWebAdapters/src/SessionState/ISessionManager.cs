// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web.Metadata;

namespace System.Web.SessionState;

public interface ISessionManager
{
    Task<ISessionState> InitializeAsync(HttpContextCore context, ISessionMetadata metadata);

    Task CompleteAsync(HttpContextCore context, ISessionState state);
}
