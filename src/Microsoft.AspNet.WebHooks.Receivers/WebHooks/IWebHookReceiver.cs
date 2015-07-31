// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for processing incoming WebHooks from a particular WebHook generator, for example
    /// <c>Dropbox</c>, <c>GitHub</c>, etc.
    /// </summary>
    public interface IWebHookReceiver
    {
        /// <summary>
        /// Gets the list of case-insensitive names of the WebHook generators that this receiver supports, for example <c>dropbox</c>.
        /// The names provided here will map to URIs of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/&lt;name&gt;</c>'.
        /// </summary>
        IEnumerable<string> Names { get; }

        /// <summary>
        /// Processes the incoming WebHook request. The request may be an initialization request or it may be 
        /// an actual WebHook request. It is up to the receiver to determine what kind of incoming request
        /// is and process it accordingly.
        /// </summary>
        /// <param name="receiver">The case-insensitive name of the receiver used by the incoming WebHook. The receiver 
        /// name can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">The <see cref="HttpRequestContext"/> for the incoming request.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> containing the incoming WebHook.</param>
        Task<HttpResponseMessage> ReceiveAsync(string receiver, HttpRequestContext context, HttpRequestMessage request);
    }
}
