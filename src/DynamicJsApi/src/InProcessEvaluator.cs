// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJS
{
    internal class InProcessEvaluator
    {
        private readonly IJSInProcessRuntime _jsRuntime;

        private readonly MethodInfo _getResultGenericMethodInfo;

        private readonly IDictionary<Type, GetResultDelegate> _cachedDelegates;

        private delegate object GetResultDelegate(long treeId, long targetObjectId, IEnumerable<object> expressionList);

        public InProcessEvaluator(IJSInProcessRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _getResultGenericMethodInfo = GetType().GetMethod(nameof(EvaluateGeneric), BindingFlags.Instance | BindingFlags.NonPublic)!;
            _cachedDelegates = new Dictionary<Type, GetResultDelegate>();
        }

        public object Evaluate(Type returnType, long treeId, long targetObjectId, IEnumerable<object> expressionList)
        {
            if (!_cachedDelegates.TryGetValue(returnType, out var getResult))
            {
                var getResultMethodInfo = _getResultGenericMethodInfo.MakeGenericMethod(returnType);
                getResult = _cachedDelegates[returnType] = (GetResultDelegate)Delegate.CreateDelegate(typeof(GetResultDelegate), this, getResultMethodInfo);
            }

            return getResult(treeId, targetObjectId, expressionList);
        }

        private object EvaluateGeneric<TValue>(long treeId, long targetObjectId, IEnumerable<object> expressionList)
            => _jsRuntime.Invoke<TValue>(DynamicJSInterop.Evaluate, treeId, targetObjectId, expressionList)!;
    }
}
