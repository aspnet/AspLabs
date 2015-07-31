// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Web.Http.Tracing;
using Microsoft.AspNet.WebHooks.Diagnostics;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides extension methods for logging.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class LogExtensions
    {
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> implementation to log with.</param>
        /// <param name="message">The message to log.</param>
        public static void Error(this ILogger log, string message)
        {
            Error(log, message, ex: null);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> implementation to log with.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="ex">Optional exception.</param>
        public static void Error(this ILogger log, string message, Exception ex)
        {
            if (log != null)
            {
                log.Log(TraceLevel.Error, message, ex);
            }
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> implementation to log with.</param>
        /// <param name="message">The message to log.</param>
        public static void Warn(this ILogger log, string message)
        {
            if (log != null)
            {
                log.Log(TraceLevel.Warn, message, ex: null);
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> implementation to log with.</param>
        /// <param name="message">The message to log.</param>
        public static void Info(this ILogger log, string message)
        {
            if (log != null)
            {
                log.Log(TraceLevel.Info, message, ex: null);
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="log">The <see cref="ILogger"/> implementation to log with.</param>
        /// <param name="message">The message to log.</param>
        public static void Debug(this ILogger log, string message)
        {
            if (log != null)
            {
                log.Log(TraceLevel.Debug, message, ex: null);
            }
        }
    }
}
