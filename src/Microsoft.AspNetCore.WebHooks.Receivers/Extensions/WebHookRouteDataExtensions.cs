// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.WebHooks;
using Microsoft.AspNetCore.WebHooks.Routing;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// Extension methods for the <see cref="RouteData"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebHookRouteDataExtensions
    {
        private static readonly string[] EventKeyNames = Enumerable.Range(0, 100)
            .Select(i => $"{WebHookConstants.EventKeyName}[{i}]")
            .ToArray();

        /// <summary>
        /// Gets an indication a WebHook receiver for the current request is configured.
        /// </summary>
        /// <param name="routeData">The <see cref="RouteData"/> for the current request.</param>
        /// <returns>
        /// <see langword="true"/> if an indication <see cref="WebHookReceiverExistsConstraint"/> ran successfully was
        /// found in the <paramref name="routeData"/>; <see langword="false"/> otherwise.
        /// </returns>
        public static bool GetWebHookReceiverExists(this RouteData routeData)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (routeData.Values.TryGetValue(WebHookConstants.ReceiverExistsKeyName, out var exists))
            {
                var receiverExists = (bool)exists;
                return receiverExists == true;
            }

            return false;
        }

        /// <summary>
        /// Gets the single WebHook event name for the current request.
        /// </summary>
        /// <param name="routeData">The <see cref="RouteData"/> for the current request.</param>
        /// <param name="eventName">Set to the event name identified in the request.</param>
        /// <returns>
        /// <see langword="true"/> if exactly one event name was found in the <paramref name="routeData"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetWebHookEventName(this RouteData routeData, out string eventName)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (routeData.Values.TryGetValue(WebHookConstants.EventKeyName, out var name))
            {
                var potentialEventName = (string)name;
                if (!string.IsNullOrEmpty(potentialEventName))
                {
                    eventName = potentialEventName;
                    return true;
                }
            }

            eventName = null;
            return false;
        }

        /// <summary>
        /// Gets the WebHook event names for the current request.
        /// </summary>
        /// <param name="routeData">The <see cref="RouteData"/> for the current request.</param>
        /// <param name="eventNames">Set to the event names identified in the request.</param>
        /// <returns>
        /// <see langword="true"/> if event names were found in the <paramref name="routeData"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetWebHookEventNames(this RouteData routeData, out string[] eventNames)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (routeData.Values.TryGetValue(WebHookConstants.EventKeyName, out var name))
            {
                var eventName = (string)name;
                if (!string.IsNullOrEmpty(eventName))
                {
                    eventNames = new[] { eventName };
                    return true;
                }
            }

            var count = 0;
            while (count < EventKeyNames.Length &&
                routeData.Values.ContainsKey(EventKeyNames[count]))
            {
                count++;
            }

            if (count != 0)
            {
                eventNames = new string[count];
                for (var i = 0; i < count; i++)
                {
                    eventNames[i] = (string)routeData.Values[EventKeyNames[i]];
                }

                return true;
            }

            eventNames = null;
            return false;
        }

        /// <summary>
        /// Gets the WebHook receiver id for the current request.
        /// </summary>
        /// <param name="routeData">The <see cref="RouteData"/> for the current request.</param>
        /// <param name="id">Set to the id of the requested receiver.</param>
        /// <returns>
        /// <see langword="true"/> if a receiver id was found in the <paramref name="routeData"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetWebHookReceiverId(this RouteData routeData, out string id)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (routeData.Values.TryGetValue(WebHookConstants.IdKeyName, out var identifier))
            {
                id = (string)identifier;
                return !string.IsNullOrEmpty(id);
            }

            id = null;
            return false;
        }

        /// <summary>
        /// Gets the WebHook receiver name for the current request.
        /// </summary>
        /// <param name="routeData">The <see cref="RouteData"/> for the current request.</param>
        /// <param name="receiverName">Set to the name of the requested receiver.</param>
        /// <returns>
        /// <see langword="true"/> if a receiver name was found in the <paramref name="routeData"/>;
        /// <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetWebHookReceiverName(this RouteData routeData, out string receiverName)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (routeData.Values.TryGetValue(WebHookConstants.ReceiverKeyName, out var receiver))
            {
                receiverName = (string)receiver;
                return !string.IsNullOrEmpty(receiverName);
            }

            receiverName = null;
            return false;
        }

        /// <summary>
        /// Stores the <paramref name="eventNames"/> for the current request in <paramref name="routeData"/>.
        /// </summary>
        /// <param name="routeData">The <see cref="RouteData"/> for the current request.</param>
        /// <param name="eventNames">The event names found in the request.</param>
        public static void SetWebHookEventNames(this RouteData routeData, StringValues eventNames)
        {
            if (routeData == null)
            {
                throw new ArgumentNullException(nameof(routeData));
            }

            if (eventNames.Count == 1)
            {
                routeData.Values[WebHookConstants.EventKeyName] = eventNames[0];
            }
            else if (eventNames.Count > 1)
            {
                for (var i = 0; i < eventNames.Count && i < EventKeyNames.Length; i++)
                {
                    routeData.Values[EventKeyNames[i]] = eventNames[i];
                }
            }
        }
    }
}
