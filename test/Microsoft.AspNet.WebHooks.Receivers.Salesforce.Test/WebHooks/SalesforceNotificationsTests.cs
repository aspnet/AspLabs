// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Microsoft.AspNet.WebHooks
{
    public class SalesforceNotificationsTests
    {
        private SalesforceNotifications _notifications1, _notifications2;
        private XElement _doc1, _doc2;

        public SalesforceNotificationsTests()
        {
            _doc1 = EmbeddedResource.ReadAsJXElement("Microsoft.AspNet.WebHooks.Messages.OutboundMessage1.xml");
            _notifications1 = new SalesforceNotifications(_doc1);

            _doc2 = EmbeddedResource.ReadAsJXElement("Microsoft.AspNet.WebHooks.Messages.OutboundMessage2.xml");
            _notifications2 = new SalesforceNotifications(_doc2);
        }

        [Fact]
        public void Document_Roundtrips()
        {
            // Act
            XElement actual = _notifications1.Document;

            // Assert
            Assert.Same(_doc1, actual);
        }

        [Fact]
        public void OrganizationId_Value_IsExtracted()
        {
            // Act
            string actual = _notifications1.OrganizationId;

            // Assert
            Assert.Equal("0123456789ABCDENNN", actual);
        }

        [Fact]
        public void OrganizationId_NoValue_IsHandled()
        {
            // Act
            string actual = _notifications2.OrganizationId;

            // Assert
            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void ActionId_Value_IsExtracted()
        {
            // Act
            string actual = _notifications1.ActionId;

            // Assert
            Assert.Equal("05h370200108OsUAAU", actual);
        }

        [Fact]
        public void ActionId_NoValue_IsHandled()
        {
            // Act
            string actual = _notifications2.ActionId;

            // Assert
            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void SessionId_Value_IsExtracted()
        {
            // Act
            string actual = _notifications1.SessionId;

            // Assert
            Assert.Equal("0123456789ABCDE!BSGSRAzG9lPxyKRPjJ3.sUQ7y0lp2hfgwX.7rEL5rIQeoYuMfw5qZNrGlNz68OF9na3K5m2amoscF3pdHU5Zep_FhBcAK1RY", actual);
        }

        [Fact]
        public void SessionId_NoValue_IsHandled()
        {
            // Act
            string actual = _notifications2.SessionId;

            // Assert
            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void EnterpriseUrl_Value_IsExtracted()
        {
            // Act
            string actual = _notifications1.EnterpriseUrl;

            // Assert
            Assert.Equal("https://na31.salesforce.com/services/Soap/c/34.0/0123456789ABCDE", actual);
        }

        [Fact]
        public void EnterpriseUrl_NoValue_IsHandled()
        {
            // Act
            string actual = _notifications2.EnterpriseUrl;

            // Assert
            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void PartnerUrl_Value_IsExtracted()
        {
            // Act
            string actual = _notifications1.PartnerUrl;

            // Assert
            Assert.Equal("https://na31.salesforce.com/services/Soap/u/34.0/0123456789ABCDE", actual);
        }

        [Fact]
        public void PartnerUrl_NoValue_IsHandled()
        {
            // Act
            string actual = _notifications2.PartnerUrl;

            // Assert
            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void Notifications_Values_AreExtracted()
        {
            // Act
            IEnumerable<Dictionary<string, string>> actual = _notifications1.Notifications;

            // Act
            Assert.Equal(3, actual.Count());
            Assert.Equal("0123456789ABCDEEAE", actual.ElementAt(0)["Id"]);
            Assert.Equal("Lead1", actual.ElementAt(0)["_NotificationType"]);
            Assert.Equal("04l37000000L0E5AAK", actual.ElementAt(0)["_NotificationId"]);

            Assert.Equal("1123456789ABCDEEAE", actual.ElementAt(1)["Id"]);
            Assert.Equal("Lead2", actual.ElementAt(1)["_NotificationType"]);
            Assert.Equal("14l37000123L0E5AAK", actual.ElementAt(1)["_NotificationId"]);

            Assert.Equal("2123456789ABCDEEAE", actual.ElementAt(2)["Id"]);
            Assert.Equal("Lead3", actual.ElementAt(2)["_NotificationType"]);
            Assert.Equal("24l37123450L0E5AAK", actual.ElementAt(2)["_NotificationId"]);
        }

        [Fact]
        public void Notifications_NoValues_AreHandled()
        {
            // Act
            IEnumerable<Dictionary<string, string>> actual = _notifications2.Notifications;

            // Act
            Assert.Empty(actual);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(":", "")]
        [InlineData("你好世界", "你好世界")]
        [InlineData("你好:世界", "世界")]
        [InlineData("sf:Lead", "Lead")]
        public void GetLocalName_Returns_ExpectedName(string qualified, string expected)
        {
            // Act
            string actual = SalesforceNotifications.GetLocalName(qualified);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
