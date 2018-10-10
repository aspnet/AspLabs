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
        /// Gets the JSON path of the property in an Azure Alert WebHook request body containing the Azure Alert event
        /// name. Matches the Application Insights rule name.
        /// </summary>
        public static string EventBodyPropertyPath => "$['context']['name']";

        /// <summary>
        /// Gets the name of the Azure Alert WebHook receiver.
        /// </summary>
        public static string ReceiverName => "azurealert";
    }
}
