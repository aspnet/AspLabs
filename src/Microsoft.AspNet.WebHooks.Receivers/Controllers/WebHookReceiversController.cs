// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Routes;

namespace Microsoft.AspNet.WebHooks.Controllers
{
    /// <summary>
    /// Accepts incoming WebHook requests and dispatches them to registered <see cref="IWebHookReceiver"/> instances.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [RoutePrefix("api/webhooks/incoming")]
    public class WebHookReceiversController : ApiController
    {
        /// <summary>
        /// Supports GET for incoming WebHook request. This is typically used to verify that a WebHook is correctly wired up.
        /// </summary>
        [Route("{webHookReceiver}/{id?}", Name = WebHookReceiverRouteNames.ReceiversAction)]
        [AllowAnonymous]
        [SuppressMessage("Microsoft.Design", "CA1026:Default parameters should not be used", Justification = "This is an established parameter binding pattern for Web API.")]
        public Task<IHttpActionResult> Get(string webHookReceiver, string id = "")
        {
            return ProcessWebHook(webHookReceiver, id);
        }

        /// <summary>
        /// Supports HEAD for incoming WebHook request. This is typically used to verify that a WebHook is correctly wired up.
        /// </summary>
        [Route("{webHookReceiver}/{id?}")]
        [AllowAnonymous]
        [SuppressMessage("Microsoft.Design", "CA1026:Default parameters should not be used", Justification = "This is an established parameter binding pattern for Web API.")]
        public Task<IHttpActionResult> Head(string webHookReceiver, string id = "")
        {
            return ProcessWebHook(webHookReceiver, id);
        }

        /// <summary>
        /// Supports POST for incoming WebHook requests. This is typically the actual WebHook.
        /// </summary>
        [Route("{webHookReceiver}/{id?}")]
        [AllowAnonymous]
        [SuppressMessage("Microsoft.Design", "CA1026:Default parameters should not be used", Justification = "This is an established parameter binding pattern for Web API.")]
        public Task<IHttpActionResult> Post(string webHookReceiver, string id = "")
        {
            return ProcessWebHook(webHookReceiver, id);
        }

        private async Task<IHttpActionResult> ProcessWebHook(string webHookReceiver, string id)
        {
            var receiverManager = Configuration.DependencyResolver.GetReceiverManager();
            var receiver = receiverManager.GetReceiver(webHookReceiver);
            if (receiver == null)
            {
                var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.ReceiverController_Unknown, webHookReceiver);
                Configuration.DependencyResolver.GetLogger().Error(message);
                return NotFound();
            }

            try
            {
                var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.ReceiverController_Processing, webHookReceiver, id);
                Configuration.DependencyResolver.GetLogger().Info(message);
                var response = await receiver.ReceiveAsync(id, RequestContext, Request);
                return ResponseMessage(response);
            }
            catch (HttpResponseException rex)
            {
                return ResponseMessage(rex.Response);
            }
            catch (Exception ex)
            {
                var inner = ex.GetBaseException();
                var message = string.Format(CultureInfo.CurrentCulture, ReceiverResources.ReceiverController_Failure, webHookReceiver, inner.Message);
                Configuration.DependencyResolver.GetLogger().Error(message, inner);
                throw;
            }
        }
    }
}
