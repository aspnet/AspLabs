// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in Azure DevOps receivers and handlers.
    /// </summary>
    public static class AzureDevOpsConstants
    {
        /// <summary>
        /// Gets the JSON path of the property in an Azure DevOps WebHook request body containing the Azure DevOps event
        /// type.
        /// </summary>
        public static string EventBodyPropertyPath => "$['eventType']";

        /// <summary>
        /// Gets the name of the Azure DevOps WebHook receiver.
        /// </summary>
        public static string ReceiverName => "azuredevops";
    }
}
