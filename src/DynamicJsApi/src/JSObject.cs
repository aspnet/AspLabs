// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Dynamic;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.DynamicJS
{
    public class JSObject : DynamicObject, IDisposable
    {
        private readonly JSExpressionTree _jsExpressionTree;

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

            if (root._jsExpressionTree.AddExpression(expression, out var result) &&
                result is JSObject jsObject)
            {
                expression.TargetObjectId = jsObject.Id;
                return jsObject;
            }
            else
            {
                throw new InvalidOperationException("An unexpected error has occurred.");
            }
        }

        internal JSObject(long id, JSExpressionTree jsExpressionTree)
        {
            Id = id;
            _jsExpressionTree = jsExpressionTree;
        }

        public override bool TryConvert(ConvertBinder binder, out object? result)
            => _jsExpressionTree.Evaluate(binder.Type, Id, out result);

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
            => _jsExpressionTree.AddExpression(new JSPropertyExpression
            {
                TargetObjectId = Id,
                Name = binder.Name
            }, out result);

        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
            => _jsExpressionTree.AddExpression(new JSMethodExpression
            {
                TargetObjectId = Id,
                Name = binder.Name,
                Args = TransformArgs(args)
            }, out result);

        public override bool TryInvoke(InvokeBinder binder, object?[]? args, out object? result)
            => _jsExpressionTree.AddExpression(new JSInvocationExpression
            {
                TargetObjectId = Id,
                Args = TransformArgs(args)
            }, out result);

        public override bool TrySetMember(SetMemberBinder binder, object? value)
            => _jsExpressionTree.AddExpression(new JSAssignmentExpression
            {
                TargetObjectId = Id,
                Name = binder.Name,
                Value = value
            }, out _);

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object? arg, out object? result)
            => AddBinaryExpression(binder.Operation, binder.ReturnType, arg, out result);

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object? result)
        {
            _jsExpressionTree.AddExpression(new JSUnaryExpression
            {
                TargetObjectId = Id,
                Operation = binder.Operation
            }, out result);

            if (binder.ReturnType != typeof(object) && binder.ReturnType != typeof(JSObject))
            {
                return _jsExpressionTree.Evaluate(binder.ReturnType, ((JSObject)result!).Id, out result);
            }

            return true;
        }

        public static bool operator ==(JSObject jsObject1, JSObject jsObject2)
        {
            jsObject1.AddBinaryExpression(ExpressionType.Equal, typeof(bool), jsObject2, out var result);
            return (bool)result!;
        }

        public static bool operator !=(JSObject jsObject1, JSObject jsObject2)
        {
            jsObject1.AddBinaryExpression(ExpressionType.NotEqual, typeof(bool), jsObject2, out var result);
            return (bool)result!;
        }

        private bool AddBinaryExpression(ExpressionType operation, Type returnType, object? arg, out object? result)
        {
            _jsExpressionTree.AddExpression(new JSBinaryExpression
            {
                TargetObjectId = Id,
                Operation = operation,
                Arg = TransformArg(arg)
            }, out result);

            if (returnType != typeof(object) && returnType != typeof(JSObject))
            {
                return _jsExpressionTree.Evaluate(returnType, ((JSObject)result!).Id, out result);
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            AddBinaryExpression(ExpressionType.Equal, typeof(bool), obj, out var result);
            return (bool)result!;
        }

        public override int GetHashCode()
            => base.GetHashCode();

        private object?[]? TransformArgs(object?[]? args)
        {
            if (args != null)
            {
                for (var i = 0; i < args.Length; i++)
                {
                    args[i] = TransformArg(args[i]);
                }
            }

            return args;
        }

        private object? TransformArg(object? arg)
            => arg switch
            {
                JSObject jsObject => new JSObjectIdWrapper { Id = jsObject.Id },
                _ => arg
            };

        public void Dispose()
        {
            if (Id == 0)
            {
                ((IDisposable)_jsExpressionTree).Dispose();
            }
            else
            {
                throw new InvalidOperationException($"Cannot dispose a non-root {nameof(JSObject)}!");
            }
        }
    }
}
