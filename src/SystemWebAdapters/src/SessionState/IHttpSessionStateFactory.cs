// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace System.Web.SessionState;

public interface IHttpSessionStateFactory
{
    Task<IHttpSessionState> CreateSessionStateAsync(HttpContextCore context);
}
