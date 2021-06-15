// Pending dotnet API review

using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading.RateLimiting
{
    public sealed class TokenBucketRateLimiter : RateLimiter
    {
        private int _permitCount;
        private int _queueCount;
        private long _lastReplenishmentTick;

        private readonly Timer? _renewTimer;
        private readonly object _lock = new object();
        private readonly TokenBucketRateLimiterOptions _options;
        private readonly Deque<RequestRegistration> _queue = new Deque<RequestRegistration>();

        private static readonly RateLimitLease SuccessfulLease = new TokenBucketLease(true, null);

        public TokenBucketRateLimiter(TokenBucketRateLimiterOptions options)
        {
            _permitCount = options.PermitLimit;
            _options = options;

            if (_options.AutoReplenishment)
            {
                _renewTimer = new Timer(Replenish, this, _options.ReplenishmentPeriod, _options.ReplenishmentPeriod);
            }
        }

        public override int GetAvailablePermits() => _permitCount;

        protected override RateLimitLease AcquireCore(int permitCount)
        {
            // These amounts of resources can never be acquired
            if (permitCount > _options.PermitLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Return SuccessfulAcquisition or FailedAcquisition depending to indicate limiter state
            if (permitCount == 0)
            {
                if (GetAvailablePermits() > 0)
                {
                    return SuccessfulLease;
                }

                return CreateFailedPermitLease();
            }

            // These amounts of resources can never be acquired
            if (Interlocked.Add(ref _permitCount, -permitCount) >= 0)
            {
                return SuccessfulLease;
            }

            Interlocked.Add(ref _permitCount, permitCount);

            return CreateFailedPermitLease();
        }

        protected override ValueTask<RateLimitLease> WaitAsyncCore(int permitCount, CancellationToken cancellationToken = default)
        {
            // These amounts of resources can never be acquired
            if (permitCount < 0 || permitCount > _options.PermitLimit)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Return SuccessfulAcquisition if requestedCount is 0 and resources are available
            if (permitCount == 0 && GetAvailablePermits() > 0)
            {
                // Perf: static failed/successful value tasks?
                return new ValueTask<RateLimitLease>(SuccessfulLease);
            }

            if (Interlocked.Add(ref _permitCount, -permitCount) >= 0)
            {
                // Perf: static failed/successful value tasks?
                return new ValueTask<RateLimitLease>(SuccessfulLease);
            }

            Interlocked.Add(ref _permitCount, permitCount);

            // Don't queue if queue limit reached
            if (_queueCount + permitCount > _options.QueueLimit)
            {
                return new ValueTask<RateLimitLease>(CreateFailedPermitLease());
            }

            var registration = new RequestRegistration(permitCount);
            _queue.EnqueueTail(registration);
            Interlocked.Add(ref _permitCount, permitCount);

            // handle cancellation
            return new ValueTask<RateLimitLease>(registration.TCS.Task);
        }

        private RateLimitLease CreateFailedPermitLease()
        {
            var replenishAmount = _permitCount - GetAvailablePermits() + _queueCount;
            var replenishPeriods = (replenishAmount / _options.TokensPerReplenishment) + 1;

            return new TokenBucketLease(false, TimeSpan.FromTicks(_options.ReplenishmentPeriod.Ticks*replenishPeriods));
        }

        public void Replenish()
        {
            if (_options.AutoReplenishment)
            {
                return;
            }
            Replenish(this);
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
            var maxPermits = options.PermitLimit;

            if (availablePermits < maxPermits)
            {
                var resoucesToAdd = Math.Min(options.TokensPerReplenishment, maxPermits - availablePermits);
                Interlocked.Add(ref limiter._permitCount, resoucesToAdd);
            }

            // Process queued requests
            var queue = limiter._queue;
            lock (limiter._lock)
            {
                while (queue.Count > 0)
                {
                    var nextPendingRequest =
                          options.QueueProcessingOrder == QueueProcessingOrder.ProcessOldest
                          ? queue.PeekHead()
                          : queue.PeekTail();

                    if (Interlocked.Add(ref limiter._permitCount, -nextPendingRequest.Count) >= 0)
                    {
                        // Request can be fulfilled
                        var request =
                            options.QueueProcessingOrder == QueueProcessingOrder.ProcessOldest
                            ? queue.DequeueHead()
                            : queue.DequeueTail();
                        Interlocked.Add(ref limiter._queueCount, -request.Count);

                        // requestToFulfill == request
                        request.TCS.SetResult(SuccessfulLease);
                    }
                    else
                    {
                        // Request cannot be fulfilled
                        Interlocked.Add(ref limiter._permitCount, nextPendingRequest.Count);
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
            public RequestRegistration(int permitCount)
            {
                Count = permitCount;
                // Use VoidAsyncOperationWithData<T> instead
                TCS = new TaskCompletionSource<RateLimitLease>();
            }

            public int Count { get; }

            public TaskCompletionSource<RateLimitLease> TCS { get; }
        }
    }
}
