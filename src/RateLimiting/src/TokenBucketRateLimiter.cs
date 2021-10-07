// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Pending dotnet API review

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Threading.RateLimiting
{
    /// <summary>
    /// <see cref="RateLimiter"/> implementation that replenishes tokens periodically instead of via a release mechanism.
    /// </summary>
    public sealed class TokenBucketRateLimiter : RateLimiter
    {
        private int _tokenCount;
        private int _queueCount;
        private long _lastReplenishmentTick = Environment.TickCount;

        private readonly Timer? _renewTimer;
        private readonly object _lock = new object();
        private readonly TokenBucketRateLimiterOptions _options;
        private readonly Deque<RequestRegistration> _queue = new Deque<RequestRegistration>();

        private static readonly RateLimitLease SuccessfulLease = new TokenBucketLease(true, null);

        /// <summary>
        /// Initializes the <see cref="TokenBucketRateLimiter"/>.
        /// </summary>
        /// <param name="options"></param>
        public TokenBucketRateLimiter(TokenBucketRateLimiterOptions options)
        {
            _tokenCount = options.TokenLimit;
            _options = options;

            if (_options.AutoReplenishment)
            {
                _renewTimer = new Timer(Replenish, this, _options.ReplenishmentPeriod, _options.ReplenishmentPeriod);
            }
        }

        /// <summary>
        /// An estimated count of available tokens.
        /// </summary>
        /// <returns></returns>
        public override int GetAvailablePermits() => _tokenCount;

        /// <inheritdoc/>
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

                return CreateFailedTokenLease(tokenCount);
            }

            lock (_lock)
            {
                if (GetAvailablePermits() >= tokenCount)
                {
                    _tokenCount -= tokenCount;
                    return SuccessfulLease;
                }

                return CreateFailedTokenLease(tokenCount);
            }
        }

        /// <inheritdoc/>
        protected override ValueTask<RateLimitLease> WaitAsyncCore(int tokenCount, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // These amounts of resources can never be acquired
            if (tokenCount > _options.TokenLimit)
            {
                throw new InvalidOperationException($"{tokenCount} token(s) exceeds the permit limit of {_options.TokenLimit}.");
            }

            // Return SuccessfulAcquisition if requestedCount is 0 and resources are available
            if (tokenCount == 0 && GetAvailablePermits() > 0)
            {
                // Perf: static failed/successful value tasks?
                return new ValueTask<RateLimitLease>(SuccessfulLease);
            }

            lock (_lock)
            {
                if (GetAvailablePermits() >= tokenCount && GetAvailablePermits() != 0)
                {
                    _tokenCount -= tokenCount;
                    // Perf: static failed/successful value tasks?
                    return new ValueTask<RateLimitLease>(SuccessfulLease);
                }

                // Don't queue if queue limit reached
                if (_queueCount + tokenCount > _options.QueueLimit)
                {
                    return new ValueTask<RateLimitLease>(CreateFailedTokenLease(tokenCount));
                }

                TaskCompletionSource<RateLimitLease> tcs = new TaskCompletionSource<RateLimitLease>(TaskCreationOptions.RunContinuationsAsynchronously);

                CancellationTokenRegistration ctr;
                if (cancellationToken.CanBeCanceled)
                {
                    ctr = cancellationToken.Register(obj => CancellationRequested((TaskCompletionSource<RateLimitLease>)obj, cancellationToken), tcs);
                }
                RequestRegistration registration = new RequestRegistration(tokenCount, tcs, ctr);
                _queue.EnqueueTail(registration);
                _queueCount += tokenCount;
                Debug.Assert(_queueCount <= _options.QueueLimit);

                // handle cancellation
                return new ValueTask<RateLimitLease>(registration.Tcs.Task);
            }
        }

        private RateLimitLease CreateFailedTokenLease(int tokenCount)
        {
            int replenishAmount = tokenCount - GetAvailablePermits() + _queueCount;
            // can't have 0 replenish periods, that would mean it should be a successful lease
            // if TokensPerPeriod is larger than the replenishAmount needed then it would be 0
            int replenishPeriods = Math.Max(replenishAmount / _options.TokensPerPeriod, 1);

            return new TokenBucketLease(false, TimeSpan.FromTicks(_options.ReplenishmentPeriod.Ticks*replenishPeriods));
        }

        /// <summary>
        /// Attempts to replenish the bucket.
        /// </summary>
        /// <returns>
        /// False if <see cref="TokenBucketRateLimiterOptions.AutoReplenishment"/> is enabled, otherwise true.
        /// Does not reflect if tokens were replenished.
        /// </returns>
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
            TokenBucketRateLimiter limiter = (state as TokenBucketRateLimiter)!;
            Debug.Assert(limiter is not null);

            // TODO: Handle wrapping? TickCount will wrap after ~24.8 days
            // Use Environment.TickCount instead of DateTime.UtcNow to avoid issues on systems where the clock can change
            long nowTicks = Environment.TickCount * TimeSpan.TicksPerMillisecond;

            // method is re-entrant (from Timer), lock to avoid multiple simultaneous replenishes
            lock (limiter!._lock)
            {
                if (nowTicks - limiter._lastReplenishmentTick < limiter._options.ReplenishmentPeriod.Ticks)
                {
                    return;
                }

                limiter._lastReplenishmentTick = nowTicks;

                int availablePermits = limiter.GetAvailablePermits();
                TokenBucketRateLimiterOptions options = limiter._options;
                int maxPermits = options.TokenLimit;
                int resourcesToAdd;

                if (availablePermits < maxPermits)
                {
                    resourcesToAdd = Math.Min(options.TokensPerPeriod, maxPermits - availablePermits);
                }
                else
                {
                    // All tokens available, nothing to do
                    return;
                }

                // Process queued requests
                Deque<RequestRegistration> queue = limiter._queue;

                limiter._tokenCount += resourcesToAdd;
                Debug.Assert(limiter._tokenCount <= limiter._options.TokenLimit);
                while (queue.Count > 0)
                {
                    RequestRegistration nextPendingRequest =
                          options.QueueProcessingOrder == QueueProcessingOrder.OldestFirst
                          ? queue.PeekHead()
                          : queue.PeekTail();

                    if (limiter.GetAvailablePermits() >= nextPendingRequest.Count)
                    {
                        // Request can be fulfilled
                        nextPendingRequest =
                            options.QueueProcessingOrder == QueueProcessingOrder.OldestFirst
                            ? queue.DequeueHead()
                            : queue.DequeueTail();

                        limiter._queueCount -= nextPendingRequest.Count;
                        limiter._tokenCount -= nextPendingRequest.Count;
                        Debug.Assert(limiter._queueCount >= 0);
                        Debug.Assert(limiter._tokenCount >= 0);

                        // requestToFulfill == request
                        if (!nextPendingRequest.Tcs.TrySetResult(SuccessfulLease))
                        {
                            // Queued item was canceled so add count back
                            limiter._tokenCount += nextPendingRequest.Count;
                        }
                        nextPendingRequest.CancellationTokenRegistration.Dispose();
                    }
                    else
                    {
                        // Request cannot be fulfilled
                        break;
                    }
                }
            }
        }

        private void CancellationRequested(TaskCompletionSource<RateLimitLease> tcs, CancellationToken token)
        {
            lock (_lock)
            {
                tcs.TrySetException(new OperationCanceledException(token));
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

            public override IEnumerable<string> MetadataNames => Enumerable();

            private IEnumerable<string> Enumerable()
            {
                if (_retryAfter is null)
                {
                    yield break;
                }

                yield return MetadataName.RetryAfter.Name;
            }

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
            public RequestRegistration(int tokenCount, TaskCompletionSource<RateLimitLease> tcs, CancellationTokenRegistration cancellationTokenRegistration)
            {
                Count = tokenCount;
                // Use VoidAsyncOperationWithData<T> instead
                Tcs = tcs;
                CancellationTokenRegistration = cancellationTokenRegistration;
            }

            public int Count { get; }

            public TaskCompletionSource<RateLimitLease> Tcs { get; }

            public CancellationTokenRegistration CancellationTokenRegistration { get; }

        }
    }
}
