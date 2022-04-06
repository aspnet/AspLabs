// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETCOREAPP3_1_OR_GREATER
using System.ComponentModel.DataAnnotations;
#endif

using System.Collections.Generic;

namespace System.Web.Adapters.SessionState;

public class RemoteAppSessionStateOptions
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
    public Uri RemoteAppUrl { get; set; } = null!;
#endif

    /// <summary>
    /// Gets or sets the cookie name that the ASP.NET framework app is expecting to hold the session id
    /// </summary>
#if NETCOREAPP3_1_OR_GREATER
    [Required]
#endif
    public string CookieName { get; set; } = "ASP.NET_SessionId";

    /// <summary>
    /// Gets the mapping of known session keys to types
    /// </summary>
    public IDictionary<string, Type> KnownKeys { get; } = new Dictionary<string, Type>();

    /// <summary>
    /// Registers a session key name to be of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    public void RegisterKey<T>(string key) => KnownKeys.Add(key, typeof(T));

#if NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// The maximum number of seconds loading session state from the remote app
    /// or committing changes to it can take before timing out.
    /// </summary>
    [Required]
    public int IOTimeoutSeconds { get; set; } = 60;
#endif
}
