// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web
{
    public static class SystemWebAdaptersExtensions
    {
        public static void AddSystemWebAdapters(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
        }

        [return: NotNullIfNotNull("context")]
        internal static HttpContext? GetAdapter(this HttpContextCore? context)
        {
            if (context is null)
            {
                return null;
            }

            var result = context.Features.Get<HttpContext>();

            if (result is null)
            {
                result = new(context);
                context.Features.Set<HttpContext>(result);
            }

            return result;
        }

        [return: NotNullIfNotNull("context")]
        internal static HttpContextCore? UnwrapAdapter(this HttpContext? context) => context;

        [return: NotNullIfNotNull("context")]
        internal static HttpContextBase? GetAdapterBase(this HttpContextCore? context)
        {
            if (context is null)
            {
                return null;
            }

            var result = context.Features.Get<HttpContextBase>();

            if (result is null)
            {
                result = new HttpContextWrapper(context);
                context.Features.Set(result);
            }

            return result!;
        }

        [return: NotNullIfNotNull("request")]
        internal static HttpRequest? GetAdapter(this HttpRequestCore? request)
            => request?.HttpContext.GetAdapter().Request;

        [return: NotNullIfNotNull("request")]
        internal static HttpRequestBase? GetAdapterBase(this HttpRequestCore? request)
            => request?.HttpContext.GetAdapterBase().Request;

        [return: NotNullIfNotNull("request")]
        internal static HttpRequestCore? UnwrapAdapter(this HttpRequest? request) => request;

        [return: NotNullIfNotNull("response")]
        internal static HttpResponse? GetAdapter(this HttpResponseCore? response)
            => response?.HttpContext.GetAdapter().Response;

        [return: NotNullIfNotNull("request")]
        internal static HttpResponseBase? GetAdapterBase(this HttpResponseCore? response)
            => response?.HttpContext.GetAdapterBase().Response;

        [return: NotNullIfNotNull("response")]
        internal static HttpResponseCore? UnwrapAdapter(this HttpResponse? response) => response;
    }
}
