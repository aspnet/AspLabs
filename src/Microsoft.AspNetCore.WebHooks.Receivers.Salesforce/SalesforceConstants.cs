// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in Salesforce receivers and handlers.
    /// </summary>
    public static class SalesforceConstants
    {
        /// <summary>
        /// Gets the XPath of the element in a Salesforce WebHook request body containing the event name.
        /// </summary>
        public static string EventNamePath =>
            "/*[local-name()='Body']/*[local-name()='notifications']/*[local-name()='ActionId']";

        /// <summary>
        /// Gets the XPath of the element in an Salesforce WebHook request body containing the Salesforce organization
        /// identifier.
        /// </summary>
        public static string OrganizationIdPath =>
            "/*[local-name()='Body']/*[local-name()='notifications']/*[local-name()='OrganizationId']";

        /// <summary>
        /// Gets the name of the Salesforce WebHook receiver.
        /// </summary>
        public static string ReceiverName => "salesforce";

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMinLength => 15;

        /// <summary>
        /// Gets the maximum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMaxLength => 18;
    }
}
