// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Provides an abstraction for a service that parses the request body. For use in filters that execute prior to
    /// regular model binding or in actions that read the request body after regular model binding.
    /// </summary>
    public interface IWebHookRequestReader
    {
        /// <summary>
        /// Read the HTTP request entity body (formatted as HTML form URL-encoded data) as an
        /// <see cref="IFormCollection"/> instance.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/> for the current request and action.</param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides an <see cref="IFormCollection"/> instance containing data
        /// from the HTTP request entity body.
        /// </returns>
        Task<IFormCollection> ReadAsFormDataAsync(ActionContext actionContext);

        /// <summary>
        /// Read the HTTP request entity body as a <typeparamref name="TModel"/> instance.
        /// </summary>
        /// <typeparam name="TModel">The type of data to return.</typeparam>
        /// <param name="actionContext">The <see cref="ActionContext"/> for the current request and action.</param>
        /// <returns>
        /// A <see cref="Task"/> that on completion provides a <typeparamref name="TModel"/> instance containing the
        /// HTTP request entity body.
        /// </returns>
        Task<TModel> ReadBodyAsync<TModel>(ActionContext actionContext);
    }
}
