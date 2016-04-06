// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks.Services;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides an abstraction for explicitly validating WebHook Ids at the time of creation as part of the 
    /// <see cref="Controllers.WebHookRegistrationsController.Post(WebHook)"/> action. By default, the unique Id
    /// identifying a <see cref="WebHook"/> is created automatically on the server side. This means that any Id provided
    /// by a client is discarded and replaced with an Id generated on server side. To enable scenarios where the client 
    /// can provide a unique <see cref="WebHook"/> Id, you can register an <see cref="IWebHookIdValidator"/> implementation
    /// through a Dependency Engine or directly via <see cref="CustomApiServices.SetIdValidator(IWebHookIdValidator)"/>. 
    /// </summary>
    public interface IWebHookIdValidator
    {
        /// <summary>
        /// Validates the <see cref="WebHook.Id"/> as provided by a client registering a new <see cref="WebHook"/>.
        /// The Id can be accepted as-is or changed as desired. If the Id is set to <c>null</c> then a valid Id 
        /// is created automatically on server side.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage"/>.</param>
        /// <param name="webHook">The incoming <see cref="WebHook"/> to inspect, manipulate, or reject.</param>
        Task ValidateIdAsync(HttpRequestMessage request, WebHook webHook);
    }
}
