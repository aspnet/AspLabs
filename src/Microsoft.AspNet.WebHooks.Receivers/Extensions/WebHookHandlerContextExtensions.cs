// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNet.WebHooks.Properties;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Various extension methods for the <see cref="WebHookHandlerContext"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookHandlerContextExtensions
    {
        /// <summary>
        /// Gets the <see cref="WebHookHandlerContext.Data"/> property as type <typeparamref name="T"/>. If the 
        /// contents is not of type <typeparamref name="T"/> then <c>null</c> is returned.
        /// </summary>
        /// <typeparam name="T">The type to convert <see cref="WebHookHandlerContext.Data"/> to.</typeparam>
        /// <param name="context">The <see cref="WebHookHandlerContext"/> to operate on.</param>
        /// <returns>An instance of type <typeparamref name="T"/> or <c>null</c> otherwise.</returns>
        public static T GetDataOrDefault<T>(this WebHookHandlerContext context)
            where T : class
        {
            if (context == null || context.Data == null)
            {
                return default(T);
            }

            if (context.Data is JToken && !typeof(JToken).IsAssignableFrom(typeof(T)))
            {
                try
                {
                    T data = ((JToken)context.Data).ToObject<T>();
                    return data;
                }
                catch (Exception ex)
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.GetDataOrDefault_Failure, context.Data.GetType(), typeof(T), ex.Message);
                    context.RequestContext.Configuration.DependencyResolver.GetLogger().Error(msg, ex);
                    return default(T);
                }
            }

            return context.Data as T;
        }

        /// <summary>
        /// Tries getting the <see cref="WebHookHandlerContext.Data"/> property as type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert <see cref="WebHookHandlerContext.Data"/> to.</typeparam>
        /// <param name="context">The <see cref="WebHookHandlerContext"/> to operate on.</param>
        /// <param name="value">The converted value.</param>
        /// <returns>An instance of type <typeparamref name="T"/> or <c>null</c> otherwise.</returns>
        public static bool TryGetData<T>(this WebHookHandlerContext context, out T value)
            where T : class
        {
            value = GetDataOrDefault<T>(context);
            return value != default(T);
        }
    }
}
