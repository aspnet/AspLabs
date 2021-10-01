// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Pending dotnet API review

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Threading.RateLimiting
{
    /// <summary>
    /// <see cref="RateLimiter"/> implementation that helps manage concurrent access to a resource.
    /// </summary>
    public sealed class ConcurrencyLimiter : RateLimiter
    {
        private int _permitCount;
        private int _queueCount;

        private readonly object _lock = new object();
        private readonly ConcurrencyLimiterOptions _options;
        private readonly Deque<RequestRegistration> _queue = new Deque<RequestRegistration>();

        private static readonly ConcurrencyLease SuccessfulLease = new ConcurrencyLease(true, null, 0);
        private static readonly ConcurrencyLease FailedLease = new ConcurrencyLease(false, null, 0);

        /// <summary>
        /// Initializes the <see cref="ConcurrencyLimiter"/>.
        /// </summary>
        /// <param name="options"></param>
        public ConcurrencyLimiter(ConcurrencyLimiterOptions options)
        {
            _options = options;
            _permitCount = _options.PermitLimit;
        }

        /// <inheritdoc/>
        public override int GetAvailablePermits() => _permitCount;

        /// <inheritdoc/>
        protected override RateLimitLease AcquireCore(int permitCount)
        {
            // These amounts of resources can never be acquired
            if (permitCount > _options.PermitLimit)
            {
                throw new InvalidOperationException($"{permitCount} permits exceeds the permit limit of {_options.PermitLimit}.");
            }

            // Return SuccessfulAcquisition or FailedAcquisition to indicate limiter state
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

        /// <inheritdoc/>
        protected override ValueTask<RateLimitLease> WaitAsyncCore(int permitCount, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // These amounts of resources can never be acquired
            if (permitCount > _options.PermitLimit)
            {
                throw new InvalidOperationException($"{permitCount} permits exceeds the permit limit of {_options.PermitLimit}.");
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
                // if permitCount is 0 we want to queue it if there are no available permits
                if (GetAvailablePermits() >= permitCount && GetAvailablePermits() != 0)
                {
                    _permitCount -= permitCount;
                    if (permitCount == 0)
                    {
                        // Edge case where the check before the lock showed 0 available permits but when we got the lock some permits were now available
                        return new ValueTask<RateLimitLease>(SuccessfulLease);
                    }
                    return new ValueTask<RateLimitLease>(new ConcurrencyLease(true, this, permitCount));
                }

                // Don't queue if queue limit reached
                if (_queueCount + permitCount > _options.QueueLimit)
                {
                    // Perf: static failed/successful value tasks?
                    return new ValueTask<RateLimitLease>(new ConcurrencyLease(false, null, 0, "Queue limit reached"));
                }

                TaskCompletionSource<RateLimitLease> tcs = new TaskCompletionSource<RateLimitLease>(TaskCreationOptions.RunContinuationsAsynchronously);
                CancellationTokenRegistration ctr;
                if (cancellationToken.CanBeCanceled)
                {
                    ctr = cancellationToken.Register(obj => CancellationRequested((TaskCompletionSource<RateLimitLease>)obj, cancellationToken), tcs);
                }

                RequestRegistration request = new RequestRegistration(permitCount, tcs, ctr);
                _queue.EnqueueTail(request);
                _queueCount += permitCount;
                Debug.Assert(_queueCount <= _options.QueueLimit);

                return new ValueTask<RateLimitLease>(request.TCS.Task);
            }
        }

        private void Release(int releaseCount)
        {
            lock (_lock) // Check lock check
            {
                _permitCount += releaseCount;
                Debug.Assert(_permitCount <=  _options.PermitLimit);

                while (_queue.Count > 0)
                {
                    RequestRegistration nextPendingRequest =
                        _options.QueueProcessingOrder == QueueProcessingOrder.OldestFirst
                        ? _queue.PeekHead()
                        : _queue.PeekTail(); 

                    if (GetAvailablePermits() >= nextPendingRequest.Count)
                    {
                        nextPendingRequest =
                            _options.QueueProcessingOrder == QueueProcessingOrder.OldestFirst
                            ? _queue.DequeueHead()
                            : _queue.DequeueTail();

                        _permitCount -= nextPendingRequest.Count;
                        _queueCount -= nextPendingRequest.Count;
                        Debug.Assert(_queueCount >= 0);
                        Debug.Assert(_permitCount >= 0);

                        var lease = nextPendingRequest.Count == 0 ? SuccessfulLease : new ConcurrencyLease(true, this, nextPendingRequest.Count);
                        // Check if request was canceled
                        if (!nextPendingRequest.TCS.TrySetResult(lease))
                        {
                            // Queued item was canceled so add count back
                            _permitCount += nextPendingRequest.Count;
                        }
                        nextPendingRequest.CancellationTokenRegistration.Dispose();
                    }
                    else
                    {
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

        private class ConcurrencyLease : RateLimitLease
        {
            private bool _disposed;
            private readonly ConcurrencyLimiter? _limiter;
            private readonly int _count;
            private readonly string? _reason;

            public ConcurrencyLease(bool isAcquired, ConcurrencyLimiter? limiter, int count, string? reason = null)
            {
                IsAcquired = isAcquired;
                _limiter = limiter;
                _count = count;
                _reason = reason;

                // No need to set the limiter if count is 0, Dispose will noop
                Debug.Assert(count == 0 ? limiter is null : true);
            }

            public override bool IsAcquired { get; }

            public override IEnumerable<string> MetadataNames => Enumerable();

            private IEnumerable<string> Enumerable()
            {
                if (_reason is null)
                {
                    yield break;
                }

                yield return MetadataName.ReasonPhrase.Name;
            }

            public override bool TryGetMetadata(string metadataName, out object? metadata)
            {
                if (_reason is not null && metadataName == MetadataName.ReasonPhrase.Name)
                {
                    metadata = _reason;
                    return true;
                }
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
            public RequestRegistration(int requestedCount, TaskCompletionSource<RateLimitLease> cts,
                CancellationTokenRegistration cancellationTokenRegistration)
            {
                Count = requestedCount;
                // Perf: Use AsyncOperation<TResult> instead
                TCS = cts;
                CancellationTokenRegistration = cancellationTokenRegistration;
            }

            public int Count { get; }

            public TaskCompletionSource<RateLimitLease> TCS { get; }

            public CancellationTokenRegistration CancellationTokenRegistration { get; }
        }
    }
}
