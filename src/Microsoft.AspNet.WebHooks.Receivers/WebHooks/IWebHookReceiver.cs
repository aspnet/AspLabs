// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        /// Gets the case-insensitive name of the WebHook generator that this receiver supports, for example <c>dropbox</c> or <c>net</c>.
        ///  The name provided here will map to a URI of the form '<c>https://&lt;host&gt;/api/webhooks/incoming/&lt;name&gt;</c>'.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Processes the incoming WebHook request. The request may be an initialization request or it may be 
        /// an actual WebHook request. It is up to the receiver to determine what kind of incoming request it
        /// is and process it accordingly.
        /// </summary>
        /// <param name="id">A (potentially empty) ID of a particular configuration for this <see cref="IWebHookReceiver"/>. This
        /// allows an <see cref="IWebHookReceiver"/> to support multiple WebHooks with individual configurations.</param>
        /// <param name="context">The <see cref="HttpRequestContext"/> for the incoming request.</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> containing the incoming WebHook.</param>
        Task<HttpResponseMessage> ReceiveAsync(string id, HttpRequestContext context, HttpRequestMessage request);
    }
}
