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

            // Do explicit conversion if Data is a JToken subtype and T is not e.g. Data is a JObject and T is
            // one of the strong types for request content.
            //
            // IsAssignableFrom(...) check looks odd because return statement effectively assigns from Data to T.
            // However, this check avoids useless "conversions" e.g. from a JObject to a JObject without attempting
            // impossible conversions e.g. from a JObject to a JArray. Json.NET does not support semantically-invalid
            // conversions between JToken subtypes.
            //
            // !typeof(T).IsAssignableFrom(context.Data.GetType()) may be more precise but is less efficient. That
            // check would not change the (null) outcome in the JObject to JArray case, just adds a first-chance
            // Exception (because the code would attempt the impossible conversion). About the only new cases it
            // handles with a cast instead of ToObject<T>() are extreme corner cases such as when T is
            // IDictionary<string, JToken> or another interface the current Data (e.g. JObject) may implement. Even
            // then, Json.NET can usually perform an explicit conversion.
            if (context.Data is JToken token && !typeof(JToken).IsAssignableFrom(typeof(T)))
            {
                try
                {
                    var data = token.ToObject<T>();
                    return data;
                }
                catch (Exception ex)
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture,
                        ReceiverResources.GetDataOrDefault_Failure,
                        context.Data.GetType(),
                        typeof(T),
                        ex.Message);
                    context.RequestContext.Configuration.DependencyResolver.GetLogger().Error(message, ex);
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
