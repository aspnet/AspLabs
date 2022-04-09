// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.Internal;

internal class HttpValueCollection : NameValueCollection
{
    internal HttpValueCollection(string? str = null, Encoding? encoding = null)
        : base(StringComparer.OrdinalIgnoreCase)
    {
        if (!string.IsNullOrEmpty(str))
        {
            FillFromString(str, true, encoding);
        }

        IsReadOnly = false;
    }

    internal void FillFromString(string s, bool urlencoded = false, Encoding? encoding = null)
    {
        var i = 0;

        while (i < s.Length)
        {
            // find next & while noting first = on the way (and if there are more)
            var si = i;
            var ti = -1;

            while (i < s.Length)
            {
                var ch = s[i];

                if (ch == '=')
                {
                    if (ti < 0)
                        ti = i;
                }
                else if (ch == '&')
                {
                    break;
                }

                i++;
            }

            // extract the name / value pair
            string? name = null;
            string? value; 

            if (ti >= 0)
            {
                name = s[si..ti];
                value = s.Substring(ti + 1, i - ti - 1);
            }
            else
            {
                value = s[si..i];
            }

            // add name / value pair to the collection
            if (urlencoded)
            {
                var (decodedName, decodedValue) = encoding is null
                    ? (HttpUtility.UrlDecode(name), HttpUtility.UrlDecode(value))
                    : (HttpUtility.UrlDecode(name, encoding), HttpUtility.UrlDecode(value, encoding));

                base.Add(decodedName, decodedValue);
            }
            else
            {
                base.Add(name, value);
            }

            // trailing '&'
            if (i == s.Length - 1 && s[i] == '&')
            {
                base.Add(null, string.Empty);
            }

            i++;
        }
    }

    public override string ToString() => ToString(false);

    private string ToString(bool urlencoded)
    {
        int count = Count;

        if (count == 0)
        {
            return string.Empty;
        }

        var s = new StringBuilder();

        for (var i = 0; i < count; i++)
        {
            var key = GetKey(i);

            if (urlencoded)
            {
                key = HttpUtility.UrlEncode(key);
            }

            var keyPrefix = string.IsNullOrEmpty(key) ? string.Empty : $"{key}=";
            var values = (ArrayList?)BaseGet(i);

            if (s.Length > 0)
            {
                s.Append('&');
            }

            if (values is null)
            {
                continue;
            }

            for (var j = 0; j < values.Count; j++)
            {
                if (j > 0)
                {
                    s.Append('&');
                }

                s.Append(keyPrefix);

                var item = (string?)values[j];

                if (urlencoded)
                {
                    item = HttpUtility.UrlEncode(item);
                }

                s.Append(item);
            }
        }

        return s.ToString();
    }
}
