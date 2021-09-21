// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Pending dotnet API review

using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading.RateLimiting
{
    public sealed class TokenBucketRateLimiter : RateLimiter
    {
        private int _tokenCount;
        private int _queueCount;
        private long _lastReplenishmentTick;

        private readonly Timer? _renewTimer;
        private readonly object _lock = new object();
        private readonly TokenBucketRateLimiterOptions _options;
        private readonly Deque<RequestRegistration> _queue = new Deque<RequestRegistration>();

        private static readonly RateLimitLease SuccessfulLease = new TokenBucketLease(true, null);

        public TokenBucketRateLimiter(TokenBucketRateLimiterOptions options)
        {
            _tokenCount = options.TokenLimit;
            _options = options;

            if (_options.AutoReplenishment)
            {
                _renewTimer = new Timer(Replenish, this, _options.ReplenishmentPeriod, _options.ReplenishmentPeriod);
            }
        }

        public override int GetAvailablePermits() => _tokenCount;

        protected override RateLimitLease AcquireCore(int tokenCount)
        {
            // These amounts of resources can never be acquired
            if (tokenCount > _options.TokenLimit)
            {
                throw new InvalidOperationException($"{tokenCount} tokens exceeds the token limit of {_options.TokenLimit}.");
            }

            // Return SuccessfulAcquisition or FailedAcquisition depending to indicate limiter state
            if (tokenCount == 0)
            {
                if (GetAvailablePermits() > 0)
                {
                    return SuccessfulLease;
                }

                return CreateFailedTokenLease();
            }

            // These amounts of resources can never be acquired
            if (Interlocked.Add(ref _tokenCount, -tokenCount) >= 0)
            {
                return SuccessfulLease;
            }

            Interlocked.Add(ref _tokenCount, tokenCount);

            return CreateFailedTokenLease();
        }

        protected override ValueTask<RateLimitLease> WaitAsyncCore(int tokenCount, CancellationToken cancellationToken = default)
        {
            // These amounts of resources can never be acquired
            if (tokenCount < 0 || tokenCount > _options.TokenLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Return SuccessfulAcquisition if requestedCount is 0 and resources are available
            if (tokenCount == 0 && GetAvailablePermits() > 0)
            {
                // Perf: static failed/successful value tasks?
                return new ValueTask<RateLimitLease>(SuccessfulLease);
            }

            if (Interlocked.Add(ref _tokenCount, -tokenCount) >= 0)
            {
                // Perf: static failed/successful value tasks?
                return new ValueTask<RateLimitLease>(SuccessfulLease);
            }

            Interlocked.Add(ref _tokenCount, tokenCount);

            // Don't queue if queue limit reached
            if (_queueCount + tokenCount > _options.QueueLimit)
            {
                return new ValueTask<RateLimitLease>(CreateFailedTokenLease());
            }

            var registration = new RequestRegistration(tokenCount);
            _queue.EnqueueTail(registration);
            Interlocked.Add(ref _tokenCount, tokenCount);

            // handle cancellation
            return new ValueTask<RateLimitLease>(registration.Tcs.Task);
        }

        private RateLimitLease CreateFailedTokenLease()
        {
            var replenishAmount = _tokenCount - GetAvailablePermits() + _queueCount;
            var replenishPeriods = (replenishAmount / _options.TokensPerPeriod) + 1;

            return new TokenBucketLease(false, TimeSpan.FromTicks(_options.ReplenishmentPeriod.Ticks*replenishPeriods));
        }

        // Attempts to replenish the bucket, returns triue if enough time has elapsed and it replenishes; otherwise, false.
        public bool TryReplenish()
        {
            if (_options.AutoReplenishment)
            {
                return false;
            }
            Replenish(this);
            return true;
        }

        private static void Replenish(object? state)
        {
            // Return if Replenish already running to avoid concurrency.
            if (!(state is TokenBucketRateLimiter))
            {
                return;
            }

            var limiter = (TokenBucketRateLimiter)state;

            var nowTicks = DateTime.Now.Ticks;
            // Need to acount for multiple periods. Need to account for ticks right below the replenishment period.
            if (nowTicks - limiter._lastReplenishmentTick < limiter._options.ReplenishmentPeriod.Ticks)
            {
                return;
            }

            limiter._lastReplenishmentTick = nowTicks;

            var availablePermits = limiter.GetAvailablePermits();
            var options = limiter._options;
            var maxPermits = options.TokenLimit;

            if (availablePermits < maxPermits)
            {
                var resoucesToAdd = Math.Min(options.TokensPerPeriod, maxPermits - availablePermits);
                Interlocked.Add(ref limiter._tokenCount, resoucesToAdd);
            }

            // Process queued requests
            var queue = limiter._queue;
            lock (limiter._lock)
            {
                while (queue.Count > 0)
                {
                    var nextPendingRequest =
                          options.QueueProcessingOrder == QueueProcessingOrder.OldestFirst
                          ? queue.PeekHead()
                          : queue.PeekTail();

                    if (Interlocked.Add(ref limiter._tokenCount, -nextPendingRequest.Count) >= 0)
                    {
                        // Request can be fulfilled
                        var request =
                            options.QueueProcessingOrder == QueueProcessingOrder.OldestFirst
                            ? queue.DequeueHead()
                            : queue.DequeueTail();
                        Interlocked.Add(ref limiter._queueCount, -request.Count);

                        // requestToFulfill == request
                        request.Tcs.SetResult(SuccessfulLease);
                    }
                    else
                    {
                        // Request cannot be fulfilled
                        Interlocked.Add(ref limiter._tokenCount, nextPendingRequest.Count);
                        break;
                    }
                }
            }
        }

        private class TokenBucketLease : RateLimitLease
        {
            private readonly TimeSpan? _retryAfter;

            public TokenBucketLease(bool isAcquired, TimeSpan? retryAfter)
            {
                IsAcquired = isAcquired;
                _retryAfter = retryAfter;
            }

            public override bool IsAcquired { get; }

            public override IEnumerable<string> MetadataNames => throw new NotImplementedException();

            public override bool TryGetMetadata(string metadataName, out object? metadata)
            {
                if (metadataName == MetadataName.RetryAfter.Name && _retryAfter.HasValue)
                {
                    metadata = _retryAfter.Value;
                    return true;
                }

                metadata = null;
                return false;
            }

            protected override void Dispose(bool disposing) { }
        }

        private struct RequestRegistration
        {
            public RequestRegistration(int tokenCount)
            {
                Count = tokenCount;
                // Use VoidAsyncOperationWithData<T> instead
                Tcs = new TaskCompletionSource<RateLimitLease>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public int Count { get; }

            public TaskCompletionSource<RateLimitLease> Tcs { get; }
        }
    }
}
