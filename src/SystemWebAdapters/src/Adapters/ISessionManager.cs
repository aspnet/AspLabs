// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace System.Web.Adapters;

/// <summary>
/// Manages creation and completion of session state.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// Creates an instance of <see cref="ISessionState"/> for a given context.
    /// </summary>
    /// <param name="context">Current <see cref="HttpContextCore"/>.</param>
    /// <param name="readOnly">Whether the session should be created read-only.</param>
    Task<ISessionState> CreateAsync(HttpContextCore context, ISessionMetadata metadata);
}
