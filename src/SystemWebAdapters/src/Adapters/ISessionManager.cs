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
    /// Creates an instance of <see cref="ISessionState"/> for a given content and metadata.
    /// </summary>
    /// <param name="context">Current <see cref="HttpContextCore"/>.</param>
    /// <param name="metadata">Metadata for the session.</param>
    Task<HttpSessionState> LoadAsync(bool readOnly);

    Task CommitAsync();
}
