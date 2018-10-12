// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Newtonsoft.Json;

namespace Microsoft.Diagnostics.Transport.Protocol
{
    public class EnableEventsRequest
    {
        public string Provider { get; set; }
        public EventLevel Level { get; set; }
        public EventKeywords Keywords { get; set; }
        public IDictionary<string, string> Arguments { get; }

        public EnableEventsRequest()
        {
        }

        public EnableEventsRequest(string provider, EventLevel level, EventKeywords keywords) : this(provider, level, keywords, arguments: null)
        {
        }

        [JsonConstructor]
        public EnableEventsRequest(string provider, EventLevel level, EventKeywords keywords, IDictionary<string, string> arguments) : this()
        {
            Provider = provider;
            Level = level;
            Keywords = keywords;
            Arguments = arguments;
        }
    }
}
