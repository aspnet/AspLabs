// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.DynamicJs
{
    internal class JSExpressionTree : IDisposable
    {
        private readonly JSObjectRuntime _jsObjectRuntime;

        private readonly long _id;

        private readonly IList<IJSExpression> _expressionList;

        private long _nextObjectId;

        private bool _disposed;

        internal JSObject Root { get; }

        public JSExpressionTree(JSObjectRuntime jsObjectRuntime, long id)
        {
            _jsObjectRuntime = jsObjectRuntime;
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

        public bool Evaluate(long targetObjectId, Type type, out object? result)
        {
            ThrowIfDisposed();

            result = _jsObjectRuntime.Evaluate(_id, targetObjectId, type,  _expressionList);

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
            _jsObjectRuntime.Evaluate(_id, -1, typeof(object), _expressionList);
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
