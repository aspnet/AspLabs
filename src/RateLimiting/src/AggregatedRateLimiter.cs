// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Pending API review

using System.Threading.Tasks;

namespace System.Threading.RateLimiting
{
#pragma warning disable 1591
    // Represent an aggregated resource (e.g. a resource limiter aggregated by IP)
    public abstract class AggregatedRateLimiter<TKey>
    {
        // an inaccurate view of resources
        public abstract int AvailablePermits(TKey resourceID);

        // Fast synchronous attempt to acquire resources
        public abstract RateLimitLease Acquire(TKey resourceID, int requestedCount);

        // Wait until the requested resources are available
        public abstract ValueTask<RateLimitLease> WaitAsync(TKey resourceID, int requestedCount, CancellationToken cancellationToken = default);
    }
#pragma warning restore
}
