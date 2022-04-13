// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Web.Internal;
using Microsoft.AspNetCore.Http;

namespace System.Web;

public sealed class HttpCookie
{
    // If the .Values collection hasn't been accessed, this will remain a string. However, once .Values is accessed,
    // this will become a HttpValueCollection. If .ToString is called on it, it will reconsistute the full string
    private object? _holder;

    public HttpCookie(string name)
    {
        Name = name;
    }

    public HttpCookie(string name, string? value)
    {
        Name = name;
        _holder = value;
    }

    /// <summary>
    /// Gets or sets the name of a cookie.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets an individual cookie value.
    public string? Value
    {
        get => _holder?.ToString();

        set
        {
            if (_holder is HttpValueCollection values)
            {
                // reset multivalue collection to contain single keyless value
                values.Clear();
                values.Add(null, value);
            }
            else
            {
                _holder = value;
            }
        }
    }

    /// <summary>
    /// Gets a collection of key/value pairs that are contained within a single cookie object.
    /// </summary>
    public NameValueCollection Values
    {
        get
        {
            if (_holder is HttpValueCollection values)
            {
                return values;
            }

            // create collection on demand
            var collection = new HttpValueCollection();

            // convert existing string value into multivalue
            if (_holder is string str)
            {
                collection.FillFromString(str);
            }

            _holder = collection;

            return collection;
        }
    }

    internal void CopyTo(HttpValueCollection other)
    {
        if (_holder is string s)
        {
            other.Add(null, s);
        }
        else if (_holder is HttpValueCollection collection)
        {
            for (var i = 0; i < collection.Count; i++)
            {
                other.Add(collection.GetKey(i), collection[i]);
            }
        }
    }

    /// <summary>
    /// Gets or sets the expiration date and time for the cookie.
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Gets or sets a value that specifies whether a cookie is accessible by client-side script.
    /// </summary>
    public bool HttpOnly { get; set; }

    /// <summary>
    /// Gets or sets the virtual path to transmit with the current cookie.
    public string Path { get; set; } = "/";

    /// <summary>
    /// Gets or sets a value indicating whether to transmit the cookie using Secure Sockets Layer (SSL)--that is, over HTTPS only.
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// Gets or sets the value for the SameSite attribute of the cookie.
    /// </summary>
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Lax;

    /// <summary>
    /// Gets or sets the domain to associate the cookie with.
    /// </summary>
    public string? Domain { get; set; }

    internal CookieOptions ToCookieOptions() => new()
    {
        Domain = Domain,
        Expires = (Expires == DateTime.MinValue) ? null : new DateTimeOffset(Expires),
        HttpOnly = HttpOnly,
        Path = Path,
        SameSite = (Microsoft.AspNetCore.Http.SameSiteMode)SameSite,
        Secure = Secure,
    };
}
