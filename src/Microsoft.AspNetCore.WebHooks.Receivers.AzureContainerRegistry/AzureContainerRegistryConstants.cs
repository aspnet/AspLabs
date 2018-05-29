// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in AzureContainerRegistry receivers and handlers.
    /// </summary>
    public static class AzureContainerRegistryConstants
    {
        /// <summary>
        /// Gets the JSON path of the property in a AzureContainerRegistry WebHook request body containing the AzureContainerRegistry event name.
        /// </summary>
        public static string EventBodyPropertyPath => "$.action";

        /// <summary>
        /// Gets the name of the AzureContainerRegistry WebHook receiver.
        /// </summary>
        public static string ReceiverName => "AzureContainerRegistry";
    }
}
