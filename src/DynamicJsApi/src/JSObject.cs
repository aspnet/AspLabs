// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.DynamicJS
{
    [JsonConverter(typeof(JSObjectJsonConverter))]
    public class JSObject : DynamicObject, IDisposable, IAsyncDisposable
    {
        private readonly JSExpressionTree _expressionTree;

        internal long Id { get; }

        public static dynamic Create(JSObject root, object value)
        {
            if (root.Id != 0)
            {
                throw new InvalidOperationException(
                    $"Can only create new {nameof(JSObject)}s from an existing root {nameof(JSObject)}.");
            }

            var expression = new JSInstantiationExpression
            {
                Value = value
            };

            var jsObject = root._expressionTree.AddExpression(expression);
            expression.TargetObjectId = jsObject.Id;

            return jsObject;
        }

        public static ValueTask<TValue> EvaluateAsync<TValue>(JSObject jsObject)
            => jsObject._expressionTree.EvaluateAsync<TValue>(jsObject.Id);

        internal JSObject(long id, JSExpressionTree jsExpressionTree)
        {
            Id = id;
            _expressionTree = jsExpressionTree;
        }

        private object ConvertTo(Type type)
        {
            if (type == typeof(object) || type == GetType())
            {
                // If the target type is unspecific or equal to the existing type, return this instance.
                return this;
            }
            else
            {
                // Otherwise, a synchronous evaluation is required. This will fail on an async expression tree.
                return _expressionTree.Evaluate(type, Id);
            }
        }

        public override bool TryConvert(ConvertBinder binder, out object? result)
        {
            result = ConvertTo(binder.Type);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = _expressionTree.AddExpression(new JSPropertyExpression
            {
                TargetObjectId = Id,
                Name = binder.Name
            });
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            result = _expressionTree.AddExpression(new JSMethodExpression
            {
                TargetObjectId = Id,
                Name = binder.Name,
                Args = args
            });
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object?[]? args, out object? result)
        {
            result = _expressionTree.AddExpression(new JSInvocationExpression
            {
                TargetObjectId = Id,
                Args = args
            });
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            _expressionTree.AddExpression(new JSAssignmentExpression
            {
                TargetObjectId = Id,
                Name = binder.Name,
                Value = value
            });
            return true;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object? arg, out object? result)
        {
            result = AddBinaryExpression(binder.Operation, binder.ReturnType, arg);
            return true;
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object? result)
        {
            result = _expressionTree.AddExpression(new JSUnaryExpression
            {
                TargetObjectId = Id,
                Operation = binder.Operation
            }).ConvertTo(binder.ReturnType);
            return true;
        }

        public static dynamic operator ==(JSObject jsObject1, JSObject jsObject2)
            => jsObject1.AddBinaryExpression(ExpressionType.Equal, typeof(object), jsObject2);

        public static dynamic operator !=(JSObject jsObject1, JSObject jsObject2)
            => jsObject1.AddBinaryExpression(ExpressionType.NotEqual, typeof(object), jsObject2);

        private object AddBinaryExpression(ExpressionType operation, Type returnType, object? arg)
            => _expressionTree.AddExpression(new JSBinaryExpression
            {
                TargetObjectId = Id,
                Operation = operation,
                Arg = arg
            }).ConvertTo(returnType);

        public void Dispose()
        {
            ThrowDisposeExceptionIfNonRoot();
            _expressionTree.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            ThrowDisposeExceptionIfNonRoot();
            return _expressionTree.DisposeAsync();
        }

        private void ThrowDisposeExceptionIfNonRoot()
        {
            if (Id != 0)
            {
                throw new InvalidOperationException($"Cannot dispose a non-root {nameof(JSObject)}.");
            }
        }
    }
}
