// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Adapters;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace System.Web
{
    public class HttpContextAdapterExtensionsTests
    {
        [Fact]
        public void NullTypesReturnNull()
        {
            Assert.Null(((HttpContextCore)null!).GetAdapter());
            Assert.Null(((HttpRequestCore)null!).GetAdapter());
            Assert.Null(((HttpResponseCore)null!).GetAdapter());

            Assert.Null(((HttpContextCore)null!).GetAdapterBase());
            Assert.Null(((HttpRequestCore)null!).GetAdapterBase());
            Assert.Null(((HttpResponseCore)null!).GetAdapterBase());

            Assert.Null(((HttpContext)null!).UnwrapAdapter());
            Assert.Null(((HttpRequest)null!).UnwrapAdapter());
            Assert.Null(((HttpResponse)null!).UnwrapAdapter());
        }

        [Fact]
        public void OriginalContextIsStored()
        {
            var context = new DefaultHttpContext();
            var adapter = new HttpContext(context);

            Assert.Same(context, adapter.UnwrapAdapter());
        }

        [Fact]
        public void AdaptersAreCached()
        {
            var context = new DefaultHttpContext();

            var contextAdapter1 = context.GetAdapter();
            var contextAdapter2 = context.GetAdapter();
            Assert.Same(contextAdapter1, contextAdapter2);

            var requestAdapter1 = context.Request.GetAdapter();
            var requestAdapter2 = context.Request.GetAdapter();
            Assert.Same(requestAdapter1, requestAdapter2);

            var responseAdapter1 = context.Response.GetAdapter();
            var responseAdapter2 = context.Response.GetAdapter();
            Assert.Same(responseAdapter1, responseAdapter2);
        }

        [Fact]
        public void AdapterHttpContextBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = context.GetAdapterBase();
            var adapterBase2 = context.GetAdapterBase();

            Assert.IsType<HttpContextWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }

        [Fact]
        public void AdapterHttpResponseBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = context.Response.GetAdapterBase();
            var adapterBase2 = context.Response.GetAdapterBase();

            Assert.IsType<HttpResponseWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }

        [Fact]
        public void AdapterHttpRequestBaseIsCached()
        {
            var context = new DefaultHttpContext();
            var adapterBase1 = context.Request.GetAdapterBase();
            var adapterBase2 = context.Request.GetAdapterBase();

            Assert.IsType<HttpRequestWrapper>(adapterBase1);
            Assert.Same(adapterBase1, adapterBase2);
        }
    }
}
