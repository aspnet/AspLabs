// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using System.Web.SessionState;

namespace System.Web.Adapters;

/// <summary>
/// Manages creation and completion of session state.
/// </summary>
public interface ISessionManager : IDisposable
{
    /// <summary>
    /// Creates an instance of <see cref="HttpSessionState"/> for a given context.
    /// </summary>
    /// <param name="context">Current <see cref="HttpContextCore"/>.</param>
    /// <param name="readOnly">Whether the session state should be loaded read-only or for read-write.</param>
    Task<HttpSessionState> LoadAsync(HttpContextCore context, bool readOnly);

    /// <summary>
    /// Commits changes made in a <see cref="HttpSessionState"/> to the remote app the session state was originally loaded from.
    /// </summary>
    /// <param name="context">Current <see cref="HttpContextCore"/>.</param>
    Task CommitAsync(HttpContextCore context);
}
