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
        private IWebHookRegistrationsManager _registrationsManager;

        /// <summary>
        /// Gets all registered WebHooks for a given user.
        /// </summary>
        /// <returns>A collection containing the registered <see cref="WebHook"/> instances for a given user.</returns>
        [Route("")]
        public async Task<IEnumerable<WebHook>> Get()
        {
            try
            {
                IEnumerable<WebHook> webHooks = await _registrationsManager.GetWebHooksAsync(User, RemovePrivateFilters);
                return webHooks;
            }
            catch (Exception ex)
            {
                Configuration.DependencyResolver.GetLogger().Error(ex.Message, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message, ex);
                throw new HttpResponseException(error);
            }
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
            try
            {
                WebHook webHook = await _registrationsManager.LookupWebHookAsync(User, id, RemovePrivateFilters);
                if (webHook != null)
                {
                    return Ok(webHook);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                Configuration.DependencyResolver.GetLogger().Error(ex.Message, ex);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message, ex);
                throw new HttpResponseException(error);
            }
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

            try
            {
                // Validate the provided WebHook ID (or force one to be created on server side)
                IWebHookIdValidator idValidator = Configuration.DependencyResolver.GetIdValidator();
                await idValidator.ValidateIdAsync(Request, webHook);

                // Validate other parts of WebHook
                await _registrationsManager.VerifySecretAsync(webHook);
                await _registrationsManager.VerifyFiltersAsync(webHook);
                await _registrationsManager.VerifyAddressAsync(webHook);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrationFailure, ex.Message);
                Configuration.DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg, ex);
                return ResponseMessage(error);
            }

            try
            {
                // Add WebHook for this user.
                StoreResult result = await _registrationsManager.AddWebHookAsync(User, webHook, AddPrivateFilters);
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

            try
            {
                // Validate parts of WebHook
                await _registrationsManager.VerifySecretAsync(webHook);
                await _registrationsManager.VerifyFiltersAsync(webHook);
                await _registrationsManager.VerifyAddressAsync(webHook);
            }
            catch (Exception ex)
            {
                string msg = string.Format(CultureInfo.CurrentCulture, CustomApiResources.RegistrationController_RegistrationFailure, ex.Message);
                Configuration.DependencyResolver.GetLogger().Info(msg);
                HttpResponseMessage error = Request.CreateErrorResponse(HttpStatusCode.BadRequest, msg, ex);
                return ResponseMessage(error);
            }

            try
            {
                // Update WebHook for this user
                StoreResult result = await _registrationsManager.UpdateWebHookAsync(User, webHook, AddPrivateFilters);
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
            try
            {
                StoreResult result = await _registrationsManager.DeleteWebHookAsync(User, id);
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
            try
            {
                await _registrationsManager.DeleteAllWebHooksAsync(User);
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

            _registrationsManager = Configuration.DependencyResolver.GetRegistrationsManager();
        }

        /// <summary>
        /// Removes all private (server side) filters from the given <paramref name="webHook"/>.
        /// </summary>
        protected virtual Task RemovePrivateFilters(string user, WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            var filters = webHook.Filters.Where(f => f.StartsWith(WebHookRegistrar.PrivateFilterPrefix, StringComparison.OrdinalIgnoreCase)).ToArray();
            foreach (string filter in filters)
            {
                webHook.Filters.Remove(filter);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// Executes all <see cref="IWebHookRegistrar"/> instances for server side manipulation, inspection, or
        /// rejection of registrations. This can for example be used to add server side only filters that 
        /// are not governed by <see cref="IWebHookFilterManager"/>.
        /// </summary>
        protected virtual async Task AddPrivateFilters(string user, WebHook webHook)
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
    }
}
