// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http.Routing;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Routes;

namespace System.Web.Http
{
    /// <summary>
    /// Provides various extensions for the <see cref="TrelloWebHookClient"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TrelloWebHookClientExtensions
    {
        /// <summary>
        /// Creates a WebHook subscription for Trello to send WebHooks when changes happen to a given Trello <paramref name="modelId"/>.
        /// </summary>
        /// <param name="client">The <see cref="TrelloWebHookClient"/> implementation.</param>
        /// <param name="urlHelper">A <see cref="UrlHelper"/> used to compute the URI where WebHooks for the given <paramref name="modelId"/> will be received.</param>
        /// <param name="modelId">The ID of a model to watch. This can be the ID of a member, card, board, or anything that actions apply to. Any event involving this model will trigger the WebHook. An example model ID is <c>4d5ea62fd76aa1136000000c</c>.</param>
        /// <param name="description">A description of the WebHook, for example <c>My Trello WebHook!</c>.</param>
        public static Task<string> CreateAsync(this TrelloWebHookClient client, UrlHelper urlHelper, string modelId, string description)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (urlHelper == null)
            {
                throw new ArgumentNullException(nameof(urlHelper));
            }

            Dictionary<string, object> parameters = new Dictionary<string, object> { { "webHookReceiver", TrelloWebHookReceiver.ReceiverName } };
            string receiver = urlHelper.Link(WebHookReceiverRouteNames.ReceiversAction, parameters);
            Uri callback = new Uri(receiver);
            return client.CreateAsync(callback, modelId, description);
        }
    }
}
