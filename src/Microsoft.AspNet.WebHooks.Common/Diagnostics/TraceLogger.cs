// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using WT = System.Web.Http.Tracing;

namespace Microsoft.AspNet.WebHooks.Diagnostics
{
    /// <summary>
    /// Provides an implementation of the <see cref="ILogger"/> interface which writes to <see cref="System.Diagnostics.Trace"/>.
    /// </summary>
    public class TraceLogger : ILogger
    {
        /// <inheritdoc />
        public void Log(WT.TraceLevel level, string message, Exception ex)
        {
            if (message == null)
            {
                return;
            }

            switch (level)
            {
                case WT.TraceLevel.Fatal:
                case WT.TraceLevel.Error:
                    Trace.TraceError(message);
                    break;

                case WT.TraceLevel.Warn:
                    Trace.TraceWarning(message);
                    break;

                case WT.TraceLevel.Info:
                    Trace.TraceInformation(message);
                    break;

                case WT.TraceLevel.Debug:
                    Trace.WriteLine(message);
                    break;

                case WT.TraceLevel.Off:
                    break;
            }
        }
    }
}
