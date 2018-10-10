// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// Extension methods with <see cref="ActionDescriptor"/> parameters for the <see cref="RouteData"/> class.
    /// </summary>
    internal static class WebHookRouteDataActionDescriptorExtensions
    {
        /// <summary>
        /// Gets the WebHook receiver id for the current request.
        /// </summary>
        /// <param name="routeData">The <see cref="RouteData"/> for the current request.</param>
        /// <param name="actionDescriptor">The <see cref="ActionDescriptor"/> for the candidate action.</param>
        /// <param name="id">Set to the id of the requested receiver.</param>
        /// <returns>
        /// <see langword="true"/> if a receiver id was found in the <paramref name="routeData"/> or
        /// <paramref name="actionDescriptor"/>; <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetWebHookReceiverId(
            this RouteData routeData,
            ActionDescriptor actionDescriptor,
            out string id)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            // Check RouteData first because we're usually dealing with the default WebHook template.
            if (routeData.TryGetWebHookReceiverId(out id))
            {
                return true;
            }

            if (actionDescriptor.RouteValues.TryGetValue(WebHookConstants.IdKeyName, out id) &&
                !string.IsNullOrEmpty(id))
            {
                return true;
            }

            id = null;
            return false;
        }

        /// <summary>
        /// Gets the WebHook receiver name for the current request.
        /// </summary>
        /// <param name="routeData">The <see cref="RouteData"/> for the current request.</param>
        /// <param name="actionDescriptor">The <see cref="ActionDescriptor"/> for the candidate action.</param>
        /// <param name="receiverName">Set to the name of the requested receiver.</param>
        /// <returns>
        /// <see langword="true"/> if a receiver name was found in the <paramref name="routeData"/> or
        /// <paramref name="actionDescriptor"/>; <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetWebHookReceiverName(
            this RouteData routeData,
            ActionDescriptor actionDescriptor,
            out string receiverName)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            // Check RouteData first because we're usually dealing with the default WebHook template.
            if (routeData.TryGetWebHookReceiverName(out receiverName))
            {
                return true;
            }

            if (actionDescriptor.RouteValues.TryGetValue(WebHookConstants.ReceiverKeyName, out receiverName) &&
                !string.IsNullOrEmpty(receiverName))
            {
                return true;
            }

            receiverName = null;
            return false;
        }
    }
}
