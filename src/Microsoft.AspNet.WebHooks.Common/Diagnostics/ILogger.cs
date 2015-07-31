// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Http.Tracing;

namespace Microsoft.AspNet.WebHooks.Diagnostics
{
    /// <summary>
    /// Provides a simple logging abstraction which can be used to pipe WebHook logs into any
    /// logging infrastructure of choice.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a <paramref name="message"/> with a given <paramref name="level"/>.
        /// </summary>
        /// <param name="level">The <see cref="TraceLevel"/> to log this message as.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="ex">An optional <see cref="Exception"/> to log.</param>
        void Log(TraceLevel level, string message, Exception ex);
    }
}
