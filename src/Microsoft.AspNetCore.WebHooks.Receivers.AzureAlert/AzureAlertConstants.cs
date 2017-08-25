// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in Azure Alert receivers and handlers.
    /// </summary>
    public static class AzureAlertConstants
    {
        /// <summary>
        /// Gets the name of the JSON property in a Azure Alert WebHook request body containing a JSON object holding
        /// the <see cref="EventRequestPropertyName"/> property.
        /// </summary>
        public static string EventRequestPropertyContainerName => "context";

        /// <summary>
        /// Gets the name of the JSON property in a Azure Alert WebHook request body containing a value somewhat
        /// analogous to an event name.
        /// </summary>
        public static string EventRequestPropertyName => "name";

        /// <summary>
        /// Gets the name of the Azure Alert WebHook receiver.
        /// </summary>
        public static string ReceiverName => "azurealert";
    }
}
