// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJS
{
    internal class BrowserSyncEvaluator : ISyncEvaluator
    {
        private readonly IJSInProcessRuntime _jsRuntime;

        private readonly MethodInfo _getResultGenericMethodInfo;

        private readonly IDictionary<Type, GetResultDelegate> _cachedDelegates;

        private delegate object GetResultDelegate(
            long treeId,
            long targetObjectId,
            IEnumerable<IJSExpression> expressionList);

        public BrowserSyncEvaluator(IJSInProcessRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _getResultGenericMethodInfo = GetType().GetMethod(
                nameof(EvaluateGeneric),
                BindingFlags.Instance | BindingFlags.NonPublic)!;
            _cachedDelegates = new Dictionary<Type, GetResultDelegate>();
        }

        public object Evaluate(
            Type returnType,
            long treeId,
            long targetObjectId,
            IEnumerable<IJSExpression> expressionList)
        {
            if (!_cachedDelegates.TryGetValue(returnType, out var getResult))
            {
                var getResultMethodInfo = _getResultGenericMethodInfo.MakeGenericMethod(returnType);
                getResult = _cachedDelegates[returnType] = (GetResultDelegate)Delegate.CreateDelegate(typeof(GetResultDelegate), this, getResultMethodInfo);
            }

            return getResult(treeId, targetObjectId, expressionList);
        }

        private object EvaluateGeneric<TResult>(
            long treeId,
            long targetObjectId,
            IEnumerable<IJSExpression> expressionList)
            => _jsRuntime.Invoke<TResult>(JSObjectInterop.Evaluate, treeId, targetObjectId, expressionList.ToList<object>())!;
    }
}
