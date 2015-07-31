// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Filters
{
    public class ValidateModelAttributeTests
    {
        private readonly ValidateModelAttribute _validater;
        private readonly HttpActionContext _actionContext;

        public ValidateModelAttributeTests()
        {
            _validater = new ValidateModelAttribute();

            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage();
            _actionContext = new HttpActionContext();
            _actionContext.ControllerContext = controllerContext;
        }

        [Fact]
        public async Task OnActionExecuting_HandlesModelStateErrors()
        {
            // Arrange
            ModelState state = new ModelState();
            state.Errors.Add(new Exception("Model Error!"));
            _actionContext.ModelState.Add("m1", state);

            // Act
            _validater.OnActionExecuting(_actionContext);
            HttpError error = await _actionContext.Response.Content.ReadAsAsync<HttpError>();

            // Assert
            Assert.NotNull(error);
        }

        [Fact]
        public void OnActionExecuting_HandlesModelStateSuccess()
        {
            // Arrange
            ModelState state = new ModelState();
            _actionContext.ModelState.Add("m1", state);

            // Act
            _validater.OnActionExecuting(_actionContext);

            // Assert
            Assert.Null(_actionContext.Response);
        }
    }
}
