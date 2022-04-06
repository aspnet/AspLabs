// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.Adapters
{
    public static class SystemWebAdaptersExtensions
    {
        public static ISystemWebAdapterBuilder AddSystemWebAdapters(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            return new Builder(services);
        }

        public static ISystemWebAdapterBuilder AddSessionManager<TManager>(this ISystemWebAdapterBuilder builder)
            where TManager : class, ISessionManager
        {
            builder.Services.AddScoped<ISessionManager, TManager>();
            return builder;
        }

        public static void UseSystemWebAdapters(this IApplicationBuilder app)
        {
            app.UseMiddleware<PreBufferRequestStreamMiddleware>();
            app.UseMiddleware<SessionMiddleware>();
            app.UseMiddleware<BufferResponseStreamMiddleware>();
        }

        /// <summary>
        /// Adds request stream buffering to the endpoint(s)
        /// </summary>
        public static TBuilder PreBufferRequestStream<TBuilder>(this TBuilder builder, IPreBufferRequestStreamMetadata? metadata = null)
            where TBuilder : IEndpointConventionBuilder
            => builder.WithMetadata(metadata ?? new PreBufferRequestStreamAttribute());

        /// <summary>
        /// Adds session support for System.Web adapters for the endpoint(s)
        /// </summary>
        public static TBuilder RequireSystemWebAdapterSession<TBuilder>(this TBuilder builder, ISessionMetadata? metadata = null)
            where TBuilder : IEndpointConventionBuilder
            => builder.WithMetadata(metadata ?? new SessionAttribute());

        /// <summary>
        /// Ensure response stream is buffered to enable synchronous actions on it for the endpoint(s)
        /// </summary>
        public static TBuilder BufferResponseStream<TBuilder>(this TBuilder builder, IBufferResponseStreamMetadata? metadata = null)
            where TBuilder : IEndpointConventionBuilder
            => builder.WithMetadata(metadata ?? new BufferResponseStreamAttribute());

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
                context.Features.Set(result);
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

        internal static IDictionary AsNonGeneric(this IDictionary<object, object?> dictionary)
             => dictionary is IDictionary d ? d : new NonGenericDictionaryWrapper(dictionary);

        internal static ICollection AsNonGeneric<T>(this ICollection<T> collection)
            => collection is ICollection c ? c : new NonGenericCollectionWrapper<T>(collection);

        internal static TFeature GetRequired<TFeature>(this IFeatureCollection features)
        {
            if (features.Get<TFeature>() is TFeature feature)
            {
                return feature;
            }

            throw new InvalidOperationException($"Feature {typeof(TFeature)} is not available");
        }

        private class Builder : ISystemWebAdapterBuilder
        {
            public Builder(IServiceCollection services)
            {
                Services = services;
            }

            public IServiceCollection Services { get; }
        }
    }
}
