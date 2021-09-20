// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;

namespace System.Threading.RateLimiting
{
    // Represents a limiter type that users interact with to determine if an operation can proceed
    public abstract class RateLimiter
    {
        // An estimated count of available permits. Potential uses include diagnostics.
        public abstract int GetAvailablePermits();

        // Fast synchronous attempt to acquire permits
        // Set permitCount to 0 to get whether permits are exhausted
        public RateLimitLease Acquire(int permitCount = 1)
        {
            if (permitCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(permitCount));
            }

            return AcquireCore(permitCount);
        }

        // Implementation
        protected abstract RateLimitLease AcquireCore(int permitCount);

        // Wait until the requested permits are available or permits can no longer be acquired
        // Set permitCount to 0 to wait until permits are replenished
        public ValueTask<RateLimitLease> WaitAsync(int permitCount = 1, CancellationToken cancellationToken = default)
        {
            if (permitCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(permitCount));
            }

            return WaitAsyncCore(permitCount, cancellationToken);
        }

        // Implementation
        protected abstract ValueTask<RateLimitLease> WaitAsyncCore(int permitCount, CancellationToken cancellationToken);
    }
}
