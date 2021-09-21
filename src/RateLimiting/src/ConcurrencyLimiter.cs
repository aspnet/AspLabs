// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Pending dotnet API review

using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Threading.RateLimiting
{
    public sealed class ConcurrencyLimiter : RateLimiter
    {
        private int _permitCount;
        private int _queueCount;

        private readonly object _lock = new object();
        private readonly ConcurrencyLimiterOptions _options;
        private readonly Deque<RequestRegistration> _queue = new Deque<RequestRegistration>();

        private static readonly ConcurrencyLease SuccessfulLease = new ConcurrencyLease(true, null, 0);
        private static readonly ConcurrencyLease FailedLease = new ConcurrencyLease(false, null, 0);

        public ConcurrencyLimiter(ConcurrencyLimiterOptions options)
        {
            _options = options;
            _permitCount = _options.PermitLimit;
        }

        public override int GetAvailablePermits() => _permitCount;

        protected override RateLimitLease AcquireCore(int permitCount)
        {
            // These amounts of resources can never be acquired
            if (permitCount > _options.PermitLimit)
            {
                throw new InvalidOperationException($"{permitCount} permits exceeds the permit limit of {_options.PermitLimit}.");
            }

            // Return SuccessfulAcquisition or FailedAcquisition depending to indicate limiter state
            if (permitCount == 0)
            {
                return GetAvailablePermits() > 0 ? SuccessfulLease : FailedLease;
            }

            // Perf: Check SemaphoreSlim implementation instead of locking
            if (GetAvailablePermits() >= permitCount)
            {
                lock (_lock)
                {
                    if (GetAvailablePermits() >= permitCount)
                    {
                        _permitCount -= permitCount;
                        return new ConcurrencyLease(true, this, permitCount);
                    }
                }
            }

            return FailedLease;
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

            // Perf: Check SemaphoreSlim implementation instead of locking
            lock (_lock) // Check lock check
            {
                if (GetAvailablePermits() >= permitCount)
                {
                    _permitCount -= permitCount;
                    return new ValueTask<RateLimitLease>(new ConcurrencyLease(true, this, permitCount));
                }

                // Don't queue if queue limit reached
                if (_queueCount + permitCount > _options.QueueLimit)
                {
                    // Perf: static failed/successful value tasks?
                    return new ValueTask<RateLimitLease>(FailedLease);
                }

                var request = new RequestRegistration(permitCount);
                _queue.EnqueueTail(request);
                _queueCount += permitCount;

                // TODO: handle cancellation
                return new ValueTask<RateLimitLease>(request.TCS.Task);
            }
        }

        private void Release(int releaseCount)
        {
            lock (_lock) // Check lock check
            {
                _permitCount += releaseCount;

                while (_queue.Count > 0)
                {
                    var nextPendingRequest =
                        _options.QueueProcessingOrder == QueueProcessingOrder.OldestFirst
                        ? _queue.PeekHead()
                        : _queue.PeekTail(); 

                    if (GetAvailablePermits() >= nextPendingRequest.Count)
                    {
                        var request =
                            _options.QueueProcessingOrder == QueueProcessingOrder.OldestFirst
                            ? _queue.DequeueHead()
                            : _queue.DequeueTail();

                        _permitCount -= request.Count;
                        _queueCount -= request.Count;

                        // requestToFulfill == request
                        request.TCS.SetResult(new ConcurrencyLease(true, this, request.Count));
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private class ConcurrencyLease : RateLimitLease
        {
            private static readonly IEnumerable<string> Empty = new string[0];

            private bool _disposed;
            private readonly ConcurrencyLimiter? _limiter;
            private readonly int _count;

            public ConcurrencyLease(bool isAcquired, ConcurrencyLimiter? limiter, int count)
            {
                IsAcquired = isAcquired;
                _limiter = limiter;
                _count = count;
            }

            public override bool IsAcquired { get; }

            public override IEnumerable<string> MetadataNames => Empty;

            public override bool TryGetMetadata(string metadataName, out object? metadata)
            {
                metadata = default;
                return false;
            }

            protected override void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                _limiter?.Release(_count);
            }
        }

        private struct RequestRegistration
        {
            public RequestRegistration(int requestedCount)
            {
                Count = requestedCount;
                // Perf: Use AsyncOperation<TResult> instead
                TCS = new TaskCompletionSource<RateLimitLease>();
            }

            public int Count { get; }

            public TaskCompletionSource<RateLimitLease> TCS { get; }
        }
    }
}
