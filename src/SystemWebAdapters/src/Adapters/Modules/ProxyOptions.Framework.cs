// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters;

public class ProxyOptions
{
    private string? _port;

    /// <summary>
    /// Gets or sets whether the X-Forwarded-* headers should be used for incoming requests.
    /// </summary>
    public bool UseForwardedHeaders { get; set; }

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string? ServerName { get; set; }

    /// <summary>
    /// Gets or sets the server port.
    /// </summary>
    public int ServerPort { get; set; } = 443;

    /// <summary>
    /// Gets or sets the scheme. 
    /// </summary>
    public string Scheme { get; set; } = "https";

    internal string ServerPortString => _port ??= ServerPort.ToString();
}
