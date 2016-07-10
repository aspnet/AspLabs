// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using Microsoft.AspNet.WebHooks.Filters;
using Microsoft.AspNet.WebHooks.Properties;
using Microsoft.AspNet.WebHooks.Routes;

namespace Microsoft.AspNet.WebHooks.Controllers
{
    /// <summary>
    /// The <see cref="WebHookRegistrationsController"/> allows the caller to create, modify, and manage WebHooks
    /// through a REST-style interface.
    /// </summary>
    [Authorize]
    [RoutePrefix("api/webhooks/registrations")]
    public class WebHookRegistrationsController : ApiController
    {
        private IWebHookManager _manager;
        private IWebHookStore _store;
        private IWebHookUser _user;

        /// <summary>
        /// Gets all registered WebHooks for a given user.
        /// </summary>
        /// <returns>A collection containing the registered <see cref="WebHook"/> instances for a given user.</returns>
        [Route("")]
        public async Task<IEnumerable<WebHook>> Get()
        {
            string userId = await GetUserId();
            IEnumerable<WebHook> webHooks = await _store.GetAllWebHooksAsync(userId);
            RemovePrivateFilters(webHooks);
            return webHooks;
        }

        /// <summary>
        /// Looks up a registered WebHook with the given <paramref name="id"/> for a given user.
        /// </summary>
        /// <returns>The registered <see cref="WebHook"/> instance for a given user.</returns>
        [Route("{id}", Name = WebHookRouteNames.RegistrationLookupAction)]
        [HttpGet]
        [ResponseType(typeof(WebHook))]
        public async Task<IHttpActionResult> Lookup(string id)
        {
            string userId = await GetUserId();
            WebHook webHook = await _store.LookupWebHookAsync(userId, id);
            if (webHook != null)
            {
                RemovePrivateFilters(new[] { webHook });
                return Ok(webHook);
            }
            return NotFound();
        }

        /// <summary>
        /// Registers a new WebHook for a given user.
        /// </summary>
        /// <param name="webHook">The <see cref="WebHook"/> to create.</param>
        [Route("")]
        [ValidateModel]
        [ResponseType(typeof(WebHook))]
        public async Task<IHttpActionResult> Post(WebHook webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }

            string userId = await GetUserId();
            await VerifyFilters(webHook);
            await VerifyWebHook(webHook);

            try
            {
                // Validate the provided WebHook ID (or force one to be created on server side)
                IWebHookIdValidator idValidator = Configuration.DependencyResolver.GetIdValidator();
                await idValidator.ValidateIdAsync(Request, webHook);

                // Add WebHook for this user.
                StoreResult result = await _store.InsertWebHookAsync(userId, webHook);
                if (result == StoreResult.Success)
                {
                    return CreatedAtRoute(WebHookRouteNames.RegistrationLookupAction, new { id = webHook.Id }, webHook);
                }
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrationFailure, ex.Message);
                Configuration.DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }

        /// <summary>
        /// Updates an existing WebHook registration.
        /// </summary>
        /// <param name="id">The WebHook ID.</param>
        /// <param name="webHook">The new <see cref="WebHook"/> to use.</param>
        [Route("{id}")]
        [ValidateModel]
        public async Task<IHttpActionResult> Put(string id, WebHook webHook)
        {
            if (webHook == null)
            {
                return BadRequest();
            }
            if (!string.Equals(id, webHook.Id, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            string userId = await GetUserId();
            await VerifyFilters(webHook);
            await VerifyWebHook(webHook);

            try
            {
                StoreResult result = await _store.UpdateWebHookAsync(userId, webHook);
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_UpdateFailure, ex.Message);
                Configuration.DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }

        /// <summary>
        /// Deletes an existing WebHook registration.
        /// </summary>
        /// <param name="id">The WebHook ID.</param>
        [Route("{id}")]
        public async Task<IHttpActionResult> Delete(string id)
        {
            string userId = await GetUserId();

            try
            {
                StoreResult result = await _store.DeleteWebHookAsync(userId, id);
                return CreateHttpResult(result);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_DeleteFailure, ex.Message);
                Configuration.DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }

        /// <summary>
        /// Deletes all existing WebHook registrations.
        /// </summary>
        [Route("")]
        public async Task<IHttpActionResult> DeleteAll()
        {
            string userId = await GetUserId();

            try
            {
                await _store.DeleteAllWebHooksAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_DeleteAllFailure, ex.Message);
                Configuration.DependencyResolver.GetLogger().Error(msg, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, msg, ex);
                return ResponseMessage(error);
            }
        }

        /// <inheritdoc />
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            _manager = Configuration.DependencyResolver.GetManager();
            _store = Configuration.DependencyResolver.GetStore();
            _user = Configuration.DependencyResolver.GetUser();
        }

        /// <summary>
        /// Ensure that the provided <paramref name="webHook"/> only has registered filters.
        /// </summary>
        protected virtual async Task VerifyFilters(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            // If there are no filters then add our wildcard filter.
            if (webHook.Filters.Count == 0)
            {
                webHook.Filters.Add(WildcardWebHookFilterProvider.Name);
                await InvokeRegistrars(webHook);
                return;
            }

            IWebHookFilterManager filterManager = Configuration.DependencyResolver.GetFilterManager();
            IDictionary<string, WebHookFilter> filters = await filterManager.GetAllWebHookFiltersAsync();
            HashSet<string> normalizedFilters = new HashSet<string>();
            List<string> invalidFilters = new List<string>();
            foreach (string filter in webHook.Filters)
            {
                WebHookFilter hookFilter;
                if (filters.TryGetValue(filter, out hookFilter))
                {
                    normalizedFilters.Add(hookFilter.Name);
                }
                else
                {
                    invalidFilters.Add(filter);
                }
            }

            if (invalidFilters.Count > 0)
            {
                string invalidFiltersMsg = string.Join(", ", invalidFilters);
                string link = Url.Link(WebHookRouteNames.FiltersGetAction, routeValues: null);
                string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_InvalidFilters, invalidFiltersMsg, link);
                Configuration.DependencyResolver.GetLogger().Info(msg);

                HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                throw new HttpResponseException(response);
            }
            else
            {
                webHook.Filters.Clear();
                foreach (string filter in normalizedFilters)
                {
                    webHook.Filters.Add(filter);
                }
            }

            await InvokeRegistrars(webHook);
        }

        /// <summary>
        /// Removes all private filters from registered WebHooks.
        /// </summary>
        protected virtual void RemovePrivateFilters(IEnumerable<WebHook> webHooks)
        {
            if (webHooks == null)
            {
                throw new ArgumentNullException("webHooks");
            }

            foreach (WebHook webHook in webHooks)
            {
                var filters = webHook.Filters.Where(f => f.StartsWith(WebHookRegistrar.PrivateFilterPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
                foreach (string filter in filters)
                {
                    webHook.Filters.Remove(filter);
                }
            }
        }

        /// <summary>
        /// Ensures that the provided <paramref name="webHook"/> has a reachable Web Hook URI unless
        /// the WebHook URI has a <c>NoEcho</c> query parameter.
        /// </summary>
        private async Task VerifyWebHook(WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException("webHook");
            }

            // If no secret is provided then we create one here. This allows for scenarios
            // where the caller may use a secret directly embedded in the WebHook URI, or
            // has some other way of enforcing security.
            if (string.IsNullOrEmpty(webHook.Secret))
            {
                webHook.Secret = Guid.NewGuid().ToString("N");
            }

            try
            {
                await _manager.VerifyWebHookAsync(webHook);
            }
            catch (Exception ex)
            {
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message, ex);
                throw new HttpResponseException(error);
            }
        }

        /// <summary>
        /// Gets the user ID for this request.
        /// </summary>
        private async Task<string> GetUserId()
        {
            try
            {
                string id = await _user.GetUserIdAsync(User);
                return id;
            }
            catch (Exception ex)
            {
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message, ex);
                throw new HttpResponseException(error);
            }
        }

        /// <summary>
        /// Creates an <see cref="IHttpActionResult"/> based on the provided <paramref name="result"/>.
        /// </summary>
        /// <param name="result">The result to use when creating the <see cref="IHttpActionResult"/>.</param>
        /// <returns>An initialized <see cref="IHttpActionResult"/>.</returns>
        private IHttpActionResult CreateHttpResult(StoreResult result)
        {
            switch (result)
            {
                case StoreResult.Success:
                    return Ok();

                case StoreResult.Conflict:
                    return Conflict();

                case StoreResult.NotFound:
                    return NotFound();

                case StoreResult.OperationError:
                    return BadRequest();

                default:
                    return InternalServerError();
            }
        }

        /// <summary>
        /// Calls all IWebHookRegistrar instances for server side manipulation, inspection, or rejection of registrations.
        /// </summary>
        private async Task InvokeRegistrars(WebHook webHook)
        {
            IEnumerable<IWebHookRegistrar> registrars = Configuration.DependencyResolver.GetRegistrars();
            foreach (IWebHookRegistrar registrar in registrars)
            {
                try
                {
                    await registrar.RegisterAsync(Request, webHook);
                }
                catch (HttpResponseException rex)
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrarStatusCode, registrar.GetType().Name, typeof(IWebHookRegistrar).Name, rex.Response.StatusCode);
                    Configuration.DependencyResolver.GetLogger().Info(msg);
                    throw;
                }
                catch (Exception ex)
                {
                    string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrarException, registrar.GetType().Name, typeof(IWebHookRegistrar).Name, ex.Message);
                    Configuration.DependencyResolver.GetLogger().Error(msg, ex);
                    HttpResponseMessage response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg);
                    throw new HttpResponseException(response);
                }
            }
        }
    }
}
