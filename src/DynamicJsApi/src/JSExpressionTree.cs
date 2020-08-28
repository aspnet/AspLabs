// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.DynamicJS
{
    internal class JSExpressionTree : IDisposable
    {
        private readonly ISyncEvaluator _syncEvaluator;

        private readonly long _id;

        private readonly IList<IJSExpression> _expressionList;

        private long _nextObjectId;

        private bool _disposed;

        internal JSObject Root { get; }

        public JSExpressionTree(ISyncEvaluator syncEvaluator, long id)
        {
            _syncEvaluator = syncEvaluator;
            _id = id;
            _expressionList = new List<IJSExpression>();
            _nextObjectId = 1;

            Root = new JSObject(0, this);
        }

        public bool AddExpression(IJSExpression expression, out object? result)
        {
            ThrowIfDisposed();

            result = new JSObject(_nextObjectId, this);

            _nextObjectId++;
            _expressionList.Add(expression);

            return true;
        }

        public bool Evaluate(Type returnType, long targetObjectId, out object? result)
        {
            ThrowIfDisposed();

            result = _syncEvaluator.Evaluate(returnType, _id, targetObjectId, _expressionList);

            _expressionList.Clear();

            return true;
        }

        void IDisposable.Dispose()
        {
            if (_disposed)
            {
                return;
            }

            // Evaluate the remaining expressions (targetObjectId of -1 clears the object cache).
            _syncEvaluator.Evaluate(typeof(object), _id, -1, _expressionList);
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
