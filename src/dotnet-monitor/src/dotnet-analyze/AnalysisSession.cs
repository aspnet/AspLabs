// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Diagnostics.Runtime;

namespace Microsoft.Diagnostics.Tools.Analyze
{
    public class AnalysisSession
    {
        public DataTarget Target { get; }
        public ClrRuntime Runtime { get; }

        public AnalysisSession(DataTarget target, ClrRuntime runtime)
        {
            Target = target;
            Runtime = runtime;
        }
    }
}
