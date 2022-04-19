// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.Specialized;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState;

internal class SessionValues : NameObjectCollectionBase
{
    public void Add(string key, object? value) => BaseAdd(key, value);

    public void Clear() => BaseClear();

    public void Remove(string key) => BaseRemove(key);

    public new IEnumerable<string> Keys
    {
        get
        {
            foreach (var key in base.Keys)
            {
                yield return (string)key!;
            }
        }
    }

    public object? this[string key]
    {
        get => BaseGet(key);
        set => BaseSet(key, value);
    }
}
