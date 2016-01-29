// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Extensions
{
    public class WebHookManagerExtensionsTests
    {
        private const string Action = "action";

        private Mock<IWebHookManager> _manager;
        private IDictionary<string, object> _parameters;
        private Func<WebHook, string, bool> _predicate;

        public WebHookManagerExtensionsTests()
        {
            _manager = new Mock<IWebHookManager>();
            _parameters = new Dictionary<string, object>
            {
                { "p1", 1234 }
            };
            _predicate = (w, s) => true;
        }

        [Fact]
        public async Task NotifyAllActionData_CallsManager()
        {
            // Act
            await _manager.Object.NotifyAllAsync(Action, _parameters);

            // Assert
            _manager.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => VerifyNotification(n)), null), Times.Once());
        }

        [Fact]
        public async Task NotifyAllActionDataPredicate_CallsManager()
        {
            // Act
            await _manager.Object.NotifyAllAsync(Action, _parameters, _predicate);

            // Assert
            _manager.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => VerifyNotification(n)), _predicate), Times.Once());
        }

        [Fact]
        public async Task NotifyAllNotifications_CallsManager()
        {
            // Arrange
            NotificationDictionary notification = new NotificationDictionary(Action, _parameters);

            // Act
            await _manager.Object.NotifyAllAsync(Action, notification);

            // Assert
            _manager.Verify(m => m.NotifyAllAsync(It.Is<IEnumerable<NotificationDictionary>>(n => VerifyNotification(n)), null), Times.Once());
        }

        private static bool VerifyNotification(IEnumerable<NotificationDictionary> notifications)
        {
            NotificationDictionary notification = notifications.First();
            return (string)notification["Action"] == "action" && (int)notification["p1"] == 1234;
        }
    }
}
