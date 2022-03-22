// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace System.Web.Internal;

internal class FormCollectionReadOnlyDictionary : IReadOnlyDictionary<string, StringValues>
{
    private readonly IFormCollection _form;

    public FormCollectionReadOnlyDictionary(IFormCollection form)
    {
        _form = form;
    }

    public StringValues this[string key] => _form[key];

    public IEnumerable<string> Keys => _form.Keys;

    public IEnumerable<StringValues> Values
    {
        get
        {
            foreach (var item in _form)
            {
                yield return item.Value;
            }
        }
    }

    public int Count => _form.Count;

    public bool ContainsKey(string key) => _form.ContainsKey(key);

    public IEnumerator<KeyValuePair<string, StringValues>> GetEnumerator() => _form.GetEnumerator();

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out StringValues value) => _form.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
