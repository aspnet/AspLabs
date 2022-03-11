// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace System.Web.Internal;

internal static class NameValueCollectionExtensions
{
    public static NameValueCollection ToNameValueCollection(this IQueryCollection query) => ToReadOnlyNameValueCollection(query.Count, query);

    public static NameValueCollection ToNameValueCollection(this IFormCollection form) => ToReadOnlyNameValueCollection(form.Count, form);

    public static NameValueCollection ToNameValueCollection(this IHeaderDictionary headers) => new StringValuesDictionaryNameValueCollection(headers);

    public static NameValueCollection ToNameValueCollection(this IServerVariablesFeature serverVariables) => new ServerVariablesNameValueCollection(serverVariables);

    private static NameValueCollection ToReadOnlyNameValueCollection(int count, IEnumerable<KeyValuePair<string, StringValues>> items)
    {
        if (count == 0)
        {
            return StringValuesNameValueCollection.Empty;
        }

        return new StringValuesNameValueCollection(items);
    }
}
