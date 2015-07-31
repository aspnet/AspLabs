// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Http.Tracing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.WebHooks.Diagnostics
{
    public class LogExtensionsTests
    {
        private readonly Mock<ILogger> _loggerMock = new Mock<ILogger>();

        [Fact]
        public void Error_Logs()
        {
            // Arrange
            Exception ex = new Exception();

            // Act
            _loggerMock.Object.Error("message", ex);

            // Assert
            _loggerMock.Verify(l => l.Log(TraceLevel.Error, "message", ex));
        }

        [Fact]
        public void Warn_Logs()
        {
            // Act
            _loggerMock.Object.Warn("message");

            // Assert
            _loggerMock.Verify(l => l.Log(TraceLevel.Warn, "message", null));
        }

        [Fact]
        public void Info_Logs()
        {
            // Act
            _loggerMock.Object.Info("message");

            // Assert
            _loggerMock.Verify(l => l.Log(TraceLevel.Info, "message", null));
        }

        [Fact]
        public void Debug_Logs()
        {
            // Act
            _loggerMock.Object.Debug("message");

            // Assert
            _loggerMock.Verify(l => l.Log(TraceLevel.Debug, "message", null));
        }
    }
}
