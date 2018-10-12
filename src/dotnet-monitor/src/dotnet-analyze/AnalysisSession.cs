// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace Microsoft.Diagnostics.Tools.Analyze
{
    public class AnalysisSession
    {
        public DataTarget Target { get; }
        public ClrRuntime Runtime { get; }
        public int ActiveThreadId { get; set; }
        public ClrThread ActiveThread => GetThread(ActiveThreadId);

        public AnalysisSession(DataTarget target, ClrRuntime runtime)
        {
            Target = target;
            Runtime = runtime;

            var firstThread = runtime.Threads.FirstOrDefault();
            if(firstThread != null)
            {
                ActiveThreadId = firstThread.ManagedThreadId;
            }
        }

        public ClrThread GetThread(int id)
        {
            return Runtime.Threads.FirstOrDefault(t => t.ManagedThreadId == id);
        }
    }
}
