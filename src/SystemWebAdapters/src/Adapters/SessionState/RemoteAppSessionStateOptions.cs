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

    public string ApiKeyHeader { get; set; } = ApiKeyHeaderName;

#if NETCOREAPP3_1_OR_GREATER
    [Required]
#endif
    public string ApiKey { get; set; } = null!;

#if NETCOREAPP3_1_OR_GREATER
    [Required]
    public Uri RemoteAppUrl { get; set; } = null!;
#endif

    public IDictionary<string, Type> KnownKeys { get; } = new Dictionary<string, Type>();

    public void RegisterKey<T>(string key) => KnownKeys.Add(key, typeof(T));

}
