// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.DynamicJS
{
    /// <summary>
    /// The .NET representation of a JavaScript object.
    /// </summary>
    [JsonConverter(typeof(JSObjectJsonConverter))]
    public class JSObject : DynamicObject, IDisposable, IAsyncDisposable
    {
        private readonly JSExpressionTree _expressionTree;

        internal long Id { get; }

        /// <summary>
        /// Creates a new <see cref="JSObject"/> instance from the provided root <see cref="JSObject"/> and .NET value.
        /// </summary>
        /// <param name="root">The root <see cref="JSObject"/> whose lifetime the new <see cref="JSObject"/> will match.</param>
        /// <param name="value">The .NET representation of the value of the new <see cref="JSObject"/>.</param>
        /// <returns>A new <see cref="JSObject"/>.</returns>
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

        /// <summary>
        /// Evaluates the value of the given <see cref="JSObject"/>.
        /// </summary>
        /// <typeparam name="TValue">The expected result type.</typeparam>
        /// <param name="jsObject">The <see cref="JSObject"/> whose value should be evaluated.</param>
        /// <returns></returns>
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

        /// <inheritdoc />
        public override bool TryConvert(ConvertBinder binder, out object? result)
        {
            result = ConvertTo(binder.Type);
            return true;
        }

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = _expressionTree.AddExpression(new JSPropertyExpression
            {
                TargetObjectId = Id,
                Name = binder.Name
            });
            return true;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override bool TryInvoke(InvokeBinder binder, object?[]? args, out object? result)
        {
            result = _expressionTree.AddExpression(new JSInvocationExpression
            {
                TargetObjectId = Id,
                Args = args
            });
            return true;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object? arg, out object? result)
        {
            result = AddBinaryExpression(binder.Operation, binder.ReturnType, arg);
            return true;
        }

        /// <inheritdoc />
        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object? result)
        {
            result = _expressionTree.AddExpression(new JSUnaryExpression
            {
                TargetObjectId = Id,
                Operation = binder.Operation
            }).ConvertTo(binder.ReturnType);
            return true;
        }

        /// <summary>
        /// Represents an equality check between two <see cref="JSObject"/> instances.
        /// </summary>
        /// <remarks>
        /// This operation does not return a <see cref="bool"/>. Instead, it returns a <see cref="JSObject"/>
        /// representing the result of the operation. Note that for this reason, <see cref="Equals(object)"/>
        /// and <see cref="operator ==(JSObject, JSObject)"/> have different behavior.
        /// </remarks>
        /// <returns>A <see cref="JSObject"/> representing the result of the operation.</returns>
        public static dynamic operator ==(JSObject jsObject1, JSObject jsObject2)
            => jsObject1.AddBinaryExpression(ExpressionType.Equal, typeof(object), jsObject2);

        /// <summary>
        /// Represents an inequality check between two <see cref="JSObject"/> instances.
        /// </summary>
        /// <remarks>
        /// This operation does not return a <see cref="bool"/>. Instead, it returns a <see cref="JSObject"/>
        /// representing the result of the operation. Note that for this reason, !<see cref="Equals(object)"/>
        /// and <see cref="operator !=(JSObject, JSObject)"/> have different behavior.
        /// </remarks>
        /// <returns>A <see cref="JSObject"/> representing the result of the operation.</returns>
        public static dynamic operator !=(JSObject jsObject1, JSObject jsObject2)
            => jsObject1.AddBinaryExpression(ExpressionType.NotEqual, typeof(object), jsObject2);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// Since a <see cref="bool"/> is returned, a synchronous evaluation is implicitly performed.
        /// For this reason, this operation is invalid in an asynchronous-only context.
        /// </remarks>
        public override bool Equals(object? obj)
            => (bool)AddBinaryExpression(ExpressionType.Equal, typeof(bool), obj);

        /// <inheritdoc />
        public override int GetHashCode()
            => base.GetHashCode();

        private object AddBinaryExpression(ExpressionType operation, Type returnType, object? arg)
            => _expressionTree.AddExpression(new JSBinaryExpression
            {
                TargetObjectId = Id,
                Operation = operation,
                Arg = arg
            }).ConvertTo(returnType);

        /// <summary>
        /// Disposes a root <see cref="JSObject"/> and all of its resources.
        /// </summary>
        /// <remarks>
        /// This operation performs a final synchronous evaluation of the remaining unevaluated expressions.
        /// For this reason, this operation is invalid in an asynchronous-only context.
        /// Use <see cref="DisposeAsync"/> to dispose a root <see cref="JSObject"/> asynchronously.
        /// </remarks>
        public void Dispose()
        {
            ThrowDisposeExceptionIfNonRoot();
            _expressionTree.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes a root <see cref="JSObject"/> and all of its resources.
        /// </summary>
        /// <remarks>
        /// This operation performs a final asynchronous evaluation of the remaining unevaluated expressions.
        /// </remarks>
        /// <returns>A <see cref="ValueTask"/> representing the completion of the operation.</returns>
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
