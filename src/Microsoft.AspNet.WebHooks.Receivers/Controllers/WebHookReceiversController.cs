// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
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
                string msg = string.Format(CultureInfo.CurrentCulture, ReceiverResources.ReceiverController_Processing, webHookReceiver, id);
                Configuration.DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage response = await receiver.ReceiveAsync(id, RequestContext, Request);
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
