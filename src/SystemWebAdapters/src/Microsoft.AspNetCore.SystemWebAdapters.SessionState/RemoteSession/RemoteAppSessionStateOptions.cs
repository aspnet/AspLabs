// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP3_1_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif

using System.Collections.Generic;
using System;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.RemoteSession;

public class RemoteAppSessionStateOptions : SessionOptions
{
    internal const string ApiKeyHeaderName = "X-SystemWebAdapter-RemoteAppSession-Key";
    internal const string ReadOnlyHeaderName = "X-SystemWebAdapter-RemoteAppSession-ReadOnly";

    /// <summary>
    /// Gets or sets the header used to store the API key
    /// </summary>
    public string ApiKeyHeader { get; set; } = ApiKeyHeaderName;

    /// <summary>
    /// Gets or sets an API key used to secure the endpoint
    /// </summary>
#if NETCOREAPP3_1_OR_GREATER
    [Required]
#endif
    public string ApiKey { get; set; } = null!;

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// Gets or sets the remote app url
    /// </summary>
    [Required]
    public Uri RemoteApp { get; set; } = null!;

    [Required]
#endif
    public string SessionEndpointPath { get; set; } = "/fallback/adapter/session";

    /// <summary>
    /// Gets or sets the cookie name that the ASP.NET framework app is expecting to hold the session id
    /// </summary>
#if NETCOREAPP3_1_OR_GREATER
    [Required]
#endif
    public string CookieName { get; set; } = "ASP.NET_SessionId";

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// The maximum time loading session state from the remote app
    /// or committing changes to it can take before timing out.
    /// </summary>
    [Required]
    public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromMinutes(1);
#endif
}
