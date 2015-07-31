// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
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
        [Route("{webHookReceiver}", Name = WebHookReceiverRouteNames.ReceiversAction)]
        [AllowAnonymous]
        public Task<IHttpActionResult> Get(string webHookReceiver)
        {
            return ProcessWebHook(webHookReceiver);
        }

        /// <summary>
        /// Supports HEAD for incoming WebHook request. This is typically used to verify that a WebHook is correctly wired up.
        /// </summary>
        [Route("{webHookReceiver}")]
        [AllowAnonymous]
        public Task<IHttpActionResult> Head(string webHookReceiver)
        {
            return ProcessWebHook(webHookReceiver);
        }

        /// <summary>
        /// Supports POST for incoming WebHook requests. This is typically the actual WebHook. 
        /// </summary>
        [Route("{webHookReceiver}")]
        [AllowAnonymous]
        public Task<IHttpActionResult> Post(string webHookReceiver)
        {
            return ProcessWebHook(webHookReceiver);
        }

        private async Task<IHttpActionResult> ProcessWebHook(string webHookReceiver)
        {
            IWebHookReceiverManager receiverManager = Configuration.DependencyResolver.GetReceiverManager();
            IWebHookReceiver receiver = receiverManager.GetReceiver(webHookReceiver);
            if (receiver == null)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.ReceiverController_Unknown, webHookReceiver);
                Configuration.DependencyResolver.GetLogger().Error(msg);
                return NotFound();
            }

            try
            {
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.ReceiverController_Processing, webHookReceiver);
                Configuration.DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage response = await receiver.ReceiveAsync(webHookReceiver, RequestContext, Request);
                return ResponseMessage(response);
            }
            catch (HttpResponseException rex)
            {
                return ResponseMessage(rex.Response);
            }
            catch (Exception ex)
            {
                ex = ex.GetBaseException();
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.ReceiverController_Failure, webHookReceiver, ex.Message);
                Configuration.DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(response);
            }
        }
    }
}
