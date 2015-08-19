// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes one or more event notifications received as an Outbound Message from Salesforce.
    /// For details about Salesforce Outbound Messages, see <c>https://help.salesforce.com/htviewhelpdoc?id=workflow_defining_outbound_messages.htm</c>. 
    /// </summary>
    public class SalesforceNotifications
    {
        private static readonly XNamespace Soap = SalesforceNamespaces.Soap;
        private static readonly XNamespace Outbound = SalesforceNamespaces.OutboundMessage;
        private static readonly XNamespace Xsi = SalesforceNamespaces.Xsi;

        private readonly XElement _doc;

        private string _organizationId;
        private string _actionId;
        private string _sessionId;
        private string _enterpriseUrl;
        private string _partnerUrl;
        private List<Dictionary<string, string>> _notifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesforceNotifications"/> with a given 
        /// <paramref name="doc"/> representing an Outbound SOAP Message received from Salesforce.
        /// </summary>
        /// <param name="doc">An Outbound SOAP Message received from Salesforce.</param>
        public SalesforceNotifications(XElement doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            _doc = doc;
        }

        /// <summary>
        /// Gets the complete Outbound SOAP Message received from Salesforce.
        /// </summary>
        public XElement Document
        {
            get
            {
                return _doc;
            }
        }

        /// <summary>
        /// Gets the 18 character Organization ID originating this Outbound Message.
        /// </summary>
        public string OrganizationId
        {
            get
            {
                if (_organizationId == null)
                {
                    _organizationId = GetNotificationsValueOrDefault("OrganizationId");
                }
                return _organizationId;
            }
        }

        /// <summary>
        /// Gets the Action ID for this Outbound Message. 
        /// The Action ID indicates the workflow rule (action) that triggers the message. 
        /// </summary>
        public string ActionId
        {
            get
            {
                if (_actionId == null)
                {
                    _actionId = GetNotificationsValueOrDefault("ActionId");
                }
                return _actionId;
            }
        }

        /// <summary>
        /// Gets the optional SessionID for this Outbound Message if included in the message.
        /// A Session ID can be used to make subsequent calls back to Salesforce.
        /// </summary>
        public string SessionId
        {
            get
            {
                if (_sessionId == null)
                {
                    _sessionId = GetNotificationsValueOrDefault("SessionId");
                }
                return _sessionId;
            }
        }

        /// <summary>
        /// Gets the enterprise URI for this Outbound Message. This is the URI to use to make calls back to 
        /// Salesforce using the enterprise WSDL. 
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "These are directly deserialized values.")]
        public string EnterpriseUrl
        {
            get
            {
                if (_enterpriseUrl == null)
                {
                    _enterpriseUrl = GetNotificationsValueOrDefault("EnterpriseUrl");
                }
                return _enterpriseUrl;
            }
        }

        /// <summary>
        /// Gets the partner URI for this Outbound Message. This is the URI to use to make calls back to 
        /// Salesforce using the partner WSDL.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "These are directly deserialized values.")]
        public string PartnerUrl
        {
            get
            {
                if (_partnerUrl == null)
                {
                    _partnerUrl = GetNotificationsValueOrDefault("PartnerUrl");
                }
                return _partnerUrl;
            }
        }

        /// <summary>
        /// Gets the collection of notifications included in this Outbound Message. Each notification
        /// is represented as a <see cref="Dictionary{TKey, TValue}"/> where <c>TKey</c> is a property
        /// name and <c>TValue</c> is the value of that property. For each notification, the Notification ID 
        /// can be found using the key <c>_NotificationId</c>. Similarly, the type of notification can be found 
        /// using the key <c>_NotificationType</c>.
        /// </summary>
        public IEnumerable<Dictionary<string, string>> Notifications
        {
            get
            {
                if (_notifications == null)
                {
                    _notifications = _doc.Element(Soap + "Body").Element(Outbound + "notifications").Elements(Outbound + "Notification")
                        .Select(n => GetNotificationValues(n).ToDictionary(x => x.Name.LocalName, x => x.Value))
                        .ToList();
                }

                return _notifications;
            }
        }

        internal static IEnumerable<XElement> GetNotificationValues(XElement notification)
        {
            // Add the notification ID
            XElement id = notification.Element(Outbound + "Id");
            XName notificationIdName = Outbound + "_NotificationId";
            XElement notificationId = new XElement(notificationIdName) { Value = id.Value };
            yield return notificationId;

            // Add the notification type
            XElement salesForceObject = notification.Element(Outbound + "sObject");
            XAttribute type = salesForceObject.Attribute(Xsi + "type");
            XName notificationTypeName = Outbound + "_NotificationType";
            XElement notificationType = new XElement(notificationTypeName) { Value = GetLocalName(type.Value) };
            yield return notificationType;

            // Add notification properties
            foreach (XElement e in salesForceObject.Elements())
            {
                yield return e;
            }
        }

        internal static string GetLocalName(string qualified)
        {
            int index = qualified != null ? qualified.IndexOf(':') : -1;
            string type = index > -1 ? qualified.Substring(index + 1) : qualified;
            return type;
        }

        private string GetNotificationsValueOrDefault(string property)
        {
            try
            {
                string value = _doc.Element(Soap + "Body").Element(Outbound + "notifications").Element(Outbound + property).Value;
                return value;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
