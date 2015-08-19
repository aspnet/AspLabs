// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Common XML namespace URIs used by Salesforce SOAP messages.
    /// </summary>
    public static class SalesforceNamespaces
    {
        /// <summary>
        /// The XML namespace URI identifying a SOAP envelope.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "XNamespace is immutable.")]
        public static readonly XNamespace Soap = "http://schemas.xmlsoap.org/soap/envelope/";

        /// <summary>
        /// The XML namespace URI identifying XSI information.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "XNamespace is immutable.")]
        public static readonly XNamespace Xsi = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>
        /// The XML namespace URI identifying a Salesforce Outbound Message. 
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "XNamespace is immutable.")]
        public static readonly XNamespace OutboundMessage = "http://soap.sforce.com/2005/09/outbound";

        /// <summary>
        /// The XML namespace URI identifying Salesforce objects.
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "XNamespace is immutable.")]
        public static readonly XNamespace Objects = "urn:sobject.enterprise.soap.sforce.com";
    }
}
