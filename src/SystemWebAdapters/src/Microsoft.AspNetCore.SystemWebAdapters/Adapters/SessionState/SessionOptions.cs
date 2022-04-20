// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState;

/// <summary>
/// An interface to register known keys for session objects.
/// </summary>
public class SessionOptions
{
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
}
