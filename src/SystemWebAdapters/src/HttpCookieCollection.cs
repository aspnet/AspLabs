// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace System.Web;

public sealed class HttpCookieCollection : NameObjectCollectionBase
{
    private const string CannotGetByIndex = "Getting cookies by index is not support";

    private readonly ICookiesAdapter _adapter;

    internal HttpCookieCollection(IResponseCookies cookies)
    {
        _adapter = new ResponseCookies(cookies);
    }

    internal HttpCookieCollection(IRequestCookieCollection cookies)
    {
        _adapter = new RequestCookies(cookies);
    }

    public string[] AllKeys => _adapter.Keys;

    public HttpCookie? this[string name] => _adapter[name];

    public HttpCookie this[int index] => throw new PlatformNotSupportedException(CannotGetByIndex);

    public void Add(HttpCookie cookie) => _adapter.Add(cookie);

    public void Set(HttpCookie cookie) => _adapter.Set(cookie);

    public HttpCookie? Get(string name) => this[name];

    public HttpCookie Get(int index) => throw new PlatformNotSupportedException(CannotGetByIndex);

    public string GetKey(int index) => throw new PlatformNotSupportedException(CannotGetByIndex);

    public void Remove(string name) => _adapter.Remove(name);

    public override int Count => _adapter.Count;

    public void Clear() => throw new PlatformNotSupportedException("Clearing cookies is not supported");

    private interface ICookiesAdapter
    {
        public HttpCookie? this[string name] { get; }

        public string[] Keys { get; }

        void Remove(string name);

        void Set(HttpCookie cookie);

        void Add(HttpCookie cookie);

        int Count { get; }
    }

    private class ResponseCookies : ICookiesAdapter
    {
        private readonly IResponseCookies _cookies;

        public ResponseCookies(IResponseCookies cookies)
        {
            _cookies = cookies;
        }

        public HttpCookie? this[string name] => throw new PlatformNotSupportedException("Getting response cookies by name is not supported");

        public string[] Keys => throw new PlatformNotSupportedException("Keys are not available");

        public int Count => throw new PlatformNotSupportedException("Count is not supported");

        public void Add(HttpCookie cookie) => _cookies.Append(cookie.Name, cookie.Value ?? string.Empty, ToCookieOptions(cookie));

        public void Remove(string name) => _cookies.Delete(name);

        private static CookieOptions ToCookieOptions(HttpCookie cookie)
        {
            return new CookieOptions
            {
                Domain = cookie.Domain,
                Expires = (cookie.Expires == DateTime.MinValue) ? null : new DateTimeOffset(cookie.Expires),
                HttpOnly = cookie.HttpOnly,
                Path = cookie.Path,
                SameSite = (Microsoft.AspNetCore.Http.SameSiteMode)cookie.SameSite,
                Secure = cookie.Secure,
            };
        }

        public void Set(HttpCookie cookie)
        {
            Remove(cookie.Name);
            Add(cookie);
        }
    }

    private class RequestCookies : ICookiesAdapter
    {
        private readonly IRequestCookieCollection _cookies;

        public RequestCookies(IRequestCookieCollection cookies)
        {
            _cookies = cookies;
        }

        public HttpCookie? this[string name] => _cookies.TryGetValue(name, out var value) ? new HttpCookie(name, value) : null;

        public string[] Keys => _cookies.Keys.ToArray();

        public int Count => _cookies.Count;

        public void Add(HttpCookie cookie) => throw new PlatformNotSupportedException("Adding cookies to request collection is not supported");

        public void Remove(string name) => throw new PlatformNotSupportedException("Removing cookie from request collection is not supported");

        public void Set(HttpCookie cookie) => throw new PlatformNotSupportedException("Setting cookie for request collection is not supported");
    }
}
