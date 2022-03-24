// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace System.Web.Internal;

internal class QueryCollectionReadOnlyDictionary : IReadOnlyDictionary<string, StringValues>
{
    private readonly IQueryCollection _query;

    public QueryCollectionReadOnlyDictionary(IQueryCollection query)
    {
        _query = query;
    }

    public StringValues this[string key] => _query[key];

    public IEnumerable<string> Keys => _query.Keys;

    public IEnumerable<StringValues> Values
    {
        get
        {
            foreach (var item in _query)
            {
                yield return item.Value;
            }
        }
    }

    public int Count => _query.Count;

    public bool ContainsKey(string key) => _query.ContainsKey(key);

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => _query.GetEnumerator();

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value) => _query.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
