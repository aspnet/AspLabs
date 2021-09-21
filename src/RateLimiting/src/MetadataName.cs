// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Threading.RateLimiting
{
    public static class MetadataName
    {
        public static MetadataName<TimeSpan> RetryAfter { get; } = Create<TimeSpan>("RETRY_AFTER");
        public static MetadataName<string> ReasonPhrase { get; } = Create<string>("REASON_PHRASE");

        public static MetadataName<T> Create<T>(string name) => new MetadataName<T>(name);
    }
}
