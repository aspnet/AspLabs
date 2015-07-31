// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.WebHooks.Routes
{
    /// <summary>
    /// Provides a set of common route names used for receiving incoming WebHooks.
    /// </summary>
    public static class WebHookReceiverRouteNames
    {
        /// <summary>
        /// Provides the name of the <see cref="Controllers.WebHookReceiversController"/> action for receiving WebHook requests.
        /// </summary>
        public const string ReceiversAction = "ReceiversAction";
    }
}
