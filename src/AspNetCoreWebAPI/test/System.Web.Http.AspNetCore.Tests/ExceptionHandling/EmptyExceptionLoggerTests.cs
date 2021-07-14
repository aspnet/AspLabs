// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Xunit;

namespace System.Web.Http.AspNetCore.ExceptionHandling
{
    public class EmptyExceptionLoggerTests
    {
        [Fact]
        public void LogAsync_ReturnsCompletedTask()
        {
            // Arrange
            IExceptionLogger product = CreateProductUnderTest();
            ExceptionLoggerContext context = CreateContext();
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            Task task = product.LogAsync(context, cancellationToken);

            // Assert
            Assert.NotNull(task);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        private static ExceptionLoggerContext CreateContext()
        {
            return new ExceptionLoggerContext(new ExceptionContext(new Exception(), ExceptionCatchBlocks.HttpServer));
        }

        private static EmptyExceptionLogger CreateProductUnderTest()
        {
            return new EmptyExceptionLogger();
        }
    }
}
