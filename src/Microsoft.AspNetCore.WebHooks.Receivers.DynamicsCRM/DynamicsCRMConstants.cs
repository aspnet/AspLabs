// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in Dynamics CRM receivers and handlers.
    /// </summary>
    public static class DynamicsCRMConstants
    {
        // ??? Some old-world receivers verify a property exists in the request body. Add an action filter for this?
        // ??? See also TODO items in SalesforceVerifyOrganizationIdFilter.
        /// <summary>
        /// Gets the name of the JSON property in a Dynamics CRM WebHook request body containing a value somewhat
        /// an event name.
        /// </summary>
        public static string EventRequestPropertyName => "MessageName";

        /// <summary>
        /// Gets the name of the Dynamics CRM WebHook receiver.
        /// </summary>
        public static string ReceiverName => "dynamicscrm";
    }
}
