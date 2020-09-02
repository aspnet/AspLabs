// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJS
{
    internal class JSExpressionTree : IDisposable, IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;

        private readonly long _id;

        private readonly InProcessEvaluator? _inProcessEvaluator;

        private readonly IList<object> _expressionList;

        private long _nextObjectId;

        private bool _disposed;

        internal JSObject Root { get; }

        public JSExpressionTree(IJSRuntime jsRuntime, long id, InProcessEvaluator? inProcessEvaluator = default)
        {
            _jsRuntime = jsRuntime;
            _id = id;
            _inProcessEvaluator = inProcessEvaluator;
            _expressionList = new List<object>();
            _nextObjectId = 1;

            Root = new JSObject(0, this);
        }

        public JSObject AddExpression(IJSExpression expression)
        {
            ThrowIfDisposed();

            var result = new JSObject(_nextObjectId, this);

            _nextObjectId++;
            _expressionList.Add(expression);

            return result;
        }

        public object Evaluate(Type returnType, long targetObjectId)
        {
            if (_inProcessEvaluator == null)
            {
                throw new InvalidOperationException(
                    $"Cannot synchronously evaluate an asynchronous JS expression tree as a {returnType}. " +
                    $"Use {nameof(JSObject.EvaluateAsync)} to evaluate the result asynchronously.");
            }

            ThrowIfDisposed();

            var result = _inProcessEvaluator.Evaluate(returnType, _id, targetObjectId, _expressionList);
            _expressionList.Clear();

            return result;
        }

        public async ValueTask<TValue> EvaluateAsync<TValue>(long targetObjectId)
        {
            ThrowIfDisposed();

            var result = await _jsRuntime.InvokeAsync<TValue>(DynamicJSInterop.Evaluate, _id, targetObjectId, _expressionList);
            _expressionList.Clear();

            return result;
        }

        public void Dispose()
        {
            if (_inProcessEvaluator == null)
            {
                throw new InvalidOperationException(
                    $"Cannot synchronously dispose an asynchronous JS expression tree. " +
                    $"Use {nameof(DisposeAsync)} to dispose the expression tree asynchronously.");
            }

            if (_disposed)
            {
                return;
            }

            // Evaluate the remaining expressions (targetObjectId of -1 clears the object cache).
            _inProcessEvaluator.Evaluate(typeof(object), _id, -1, _expressionList);
            _disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            // Evaluate the remaining expressions (targetObjectId of -1 clears the object cache).
            await _jsRuntime.InvokeAsync<object>(DynamicJSInterop.Evaluate, _id, -1, _expressionList);
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    $"Cannot perform {nameof(JSObject)} operations after the root " +
                    $"{nameof(JSObject)} has been disposed!");
            }
        }
    }
}
