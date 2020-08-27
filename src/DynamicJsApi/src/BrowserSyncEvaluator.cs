// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.JSInterop;

namespace Microsoft.AspNetCore.DynamicJs
{
    internal class BrowserSyncEvaluator : ISyncEvaluator
    {
        private readonly MethodInfo _getResultGenericMethodInfo;

        private readonly IDictionary<Type, GetResultDelegate> _cachedDelegates;

        private delegate object GetResultDelegate(
            long treeId,
            long targetObjectId,
            IJSInProcessRuntime jsRuntime,
            IEnumerable<IJSExpression> expressionList);

        public BrowserSyncEvaluator()
        {
            _getResultGenericMethodInfo = GetType().GetMethod(
                nameof(EvaluateGeneric),
                BindingFlags.Static | BindingFlags.NonPublic)!;

            _cachedDelegates = new Dictionary<Type, GetResultDelegate>();
        }

        public object Evaluate(
            long treeId,
            long targetObjectId,
            Type type,
            IJSRuntime jsRuntime,
            IEnumerable<IJSExpression> expressionList)
        {
            if (!_cachedDelegates.TryGetValue(type, out var getResult))
            {
                var getResultMethodInfo = _getResultGenericMethodInfo.MakeGenericMethod(type);
                getResult = _cachedDelegates[type] = (GetResultDelegate)Delegate.CreateDelegate(typeof(GetResultDelegate), getResultMethodInfo);
            }

            return getResult(treeId, targetObjectId, (IJSInProcessRuntime)jsRuntime, expressionList);
        }

        private static object EvaluateGeneric<TResult>(
            long treeId,
            long targetObjectId,
            IJSInProcessRuntime jsRuntime,
            IEnumerable<IJSExpression> expressionList)
        {
            return jsRuntime.Invoke<TResult>(JSObjectInterop.Evaluate, treeId, targetObjectId, expressionList.ToList<object>())!;
        }
    }
}
