// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Net.Http.Headers;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Config;

namespace System.Web.Http
{
    /// <summary>
    /// Extension methods for <see cref="HttpConfiguration"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Initializes support for receiving MyGet WebHooks.
        /// A sample WebHook URI is '<c>https://&lt;host&gt;/api/webhooks/incoming/myget/{id}?code=83699ec7c1d794c0c780e49a5c72972590571fd8</c>'.
        /// For security reasons the WebHook URI must be an <c>https</c> URI and contain a 'code' query parameter with the
        /// same value as configured in the '<c>MS_WebHookReceiverSecret_MyGet</c>' application setting.
        /// The 'code' parameter must be between 32 and 128 characters long.
        /// For details about MyGet WebHooks, see <c>http://docs.myget.org/docs/reference/webhooks</c>.
        /// </summary>
        /// <param name="config">The current <see cref="HttpConfiguration"/>config.</param>
        public static void InitializeReceiveMyGetWebHooks(this HttpConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            WebHooksConfig.Initialize(config);

            // Register media type with JSON formatter
            MediaTypeHeaderValue mediaType = MediaTypeHeaderValue.Parse(MyGetWebHookReceiver.MediaType);
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(mediaType);
        }
    }
}
