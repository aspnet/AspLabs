// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters;

public enum SessionBehavior
{
    /// <summary>
    /// No session will be available on the endpoint.
    /// </summary>
    None,

    /// <summary>
    /// Asynchronously loads the session for controllers with this attribute before running the controller.
    /// </summary>
    PreLoad,

    /// <summary>
    /// Synchronously loads the session for controllers with this attribute on first use.
    /// </summary>
    [Obsolete("This will enable session on the endpoint but will resort to sync over async behavior")]
    OnDemand,
}
