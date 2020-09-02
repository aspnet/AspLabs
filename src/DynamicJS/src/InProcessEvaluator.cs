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

        private readonly MethodInfo _evaluateGenericMethodInfo;

        private readonly IDictionary<Type, EvaluateDelegate> _cachedDelegates;

        private delegate object EvaluateDelegate(long treeId, long targetObjectId, IEnumerable<object> expressionList);

        public InProcessEvaluator(IJSInProcessRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _evaluateGenericMethodInfo = GetType().GetMethod(nameof(EvaluateGeneric), BindingFlags.Instance | BindingFlags.NonPublic)!;
            _cachedDelegates = new Dictionary<Type, EvaluateDelegate>();
        }

        public object Evaluate(Type returnType, long treeId, long targetObjectId, IEnumerable<object> expressionList)
        {
            if (!_cachedDelegates.TryGetValue(returnType, out var evaluate))
            {
                var evaluateMethodInfo = _evaluateGenericMethodInfo.MakeGenericMethod(returnType);
                evaluate = _cachedDelegates[returnType] = (EvaluateDelegate)Delegate.CreateDelegate(typeof(EvaluateDelegate), this, evaluateMethodInfo);
            }

            return evaluate(treeId, targetObjectId, expressionList);
        }

        private object EvaluateGeneric<TValue>(long treeId, long targetObjectId, IEnumerable<object> expressionList)
            => _jsRuntime.Invoke<TValue>(DynamicJSInterop.Evaluate, treeId, targetObjectId, expressionList)!;
    }
}
