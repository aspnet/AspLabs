// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace System.Web.Internal;

internal static class NameValueCollectionExtensions
{
    public static NameValueCollection ToNameValueCollection(this IQueryCollection query)
    {
        if (query.Count == 0)
        {
            return StringValuesReadOnlyDictionaryNameValueCollection.Empty;
        }

        return new StringValuesReadOnlyDictionaryNameValueCollection(new QueryCollectionReadOnlyDictionary(query));
    }

    public static NameValueCollection ToNameValueCollection(this IFormCollection form)
    {
        if (form.Count == 0)
        {
            return StringValuesReadOnlyDictionaryNameValueCollection.Empty;
        }

        return new StringValuesReadOnlyDictionaryNameValueCollection(new FormCollectionReadOnlyDictionary(form));
    }

    public static NameValueCollection ToNameValueCollection(this IHeaderDictionary headers) => new StringValuesDictionaryNameValueCollection(headers);

    public static NameValueCollection ToNameValueCollection(this IServerVariablesFeature serverVariables) => new ServerVariablesNameValueCollection(serverVariables);
}
