// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    public abstract class WebHookConstraintTestBase
    {
        public static TheoryData<string> MatchingDataSet
        {
            get
            {
                return new TheoryData<string>
                {
                    "match",
                    "Match",
                    "MATCH",
                };
            }
        }

        public static TheoryData<string> NonMatchingDataSet
        {
            get
            {
                return new TheoryData<string>
                {
                    "notMatch",
                    "matching",
                    "another",
                };
            }
        }

        public static TheoryData<string> NoRequestValueDataSet
        {
            get
            {
                return new TheoryData<string>
                {
                    null,
                    string.Empty,
                };
            }
        }

        protected abstract string KeyName { get; }

        [Fact]
        public void Accept_FailsOrNoops_OnEmpty()
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);

            // Act & Assert
            ConfirmRequestValueNonMatch(context, c => constraint.Accept(c));
        }

        [Theory]
        [MemberData(nameof(MatchingDataSet))]
        public void Accept_Succeeds_OnActionDescriptorMatch(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.CurrentCandidate.Action.RouteValues.Add(KeyName, requestValue);

            // Act
            var result = constraint.Accept(context);

            // Assert
            ConfirmRequestValueMatch(context, result);
        }

        [Theory]
        [MemberData(nameof(NonMatchingDataSet))]
        public void Accept_FailsOrNoops_OnActionDescriptorNonMatch(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.CurrentCandidate.Action.RouteValues.Add(KeyName, requestValue);

            // Act
            var result = constraint.Accept(context);

            // Assert
            ConfirmRequestValueNonMatch(context, result);
        }

        [Theory]
        [MemberData(nameof(NoRequestValueDataSet))]
        public void Accept_FailsOrNoops_OnNoActionDescriptorRequestValue(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.CurrentCandidate.Action.RouteValues.Add(KeyName, requestValue);

            // Act & Assert
            ConfirmRequestValueNonMatch(context, c => constraint.Accept(c));
        }

        // RouteContext has precedence over ActionDescsriptor.
        [Fact]
        public void Accept_FailsOrNoops_OnRouteContextNonMatch_ActionDescriptorMatch()
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(KeyName, "another");
            context.CurrentCandidate.Action.RouteValues.Add(KeyName, "match");

            // Act
            var result = constraint.Accept(context);

            // Assert
            ConfirmRequestValueNonMatch(context, result);
        }

        [Theory]
        [MemberData(nameof(MatchingDataSet))]
        public void Accept_Succeeds_OnRouteContextMatch(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(KeyName, requestValue);

            // Act
            var result = constraint.Accept(context);

            // Assert
            ConfirmRequestValueMatch(context, result);
        }

        [Theory]
        [MemberData(nameof(NonMatchingDataSet))]
        public void Accept_FailsOrNoops_OnRouteContextNonMatch(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(KeyName, requestValue);

            // Act
            var result = constraint.Accept(context);

            // Assert
            ConfirmRequestValueNonMatch(context, result);
        }

        [Theory]
        [MemberData(nameof(NoRequestValueDataSet))]
        public void Accept_FailsOrNoops_OnNoRouteContextRequestValue(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(KeyName, requestValue);

            // Act & Assert
            ConfirmRequestValueNonMatch(context, c => constraint.Accept(c));
        }

        protected abstract IActionConstraint GetConstraint();

        protected virtual void ConfirmRequestValueMatch(ActionConstraintContext context, bool result)
        {
            // Most constraints succeed by allowing action selection.
            Assert.True(result);
        }

        protected virtual void ConfirmRequestValueNonMatch(ActionConstraintContext context, bool result)
        {
            // Most constraints fail by simply disallowing action selection.
            Assert.False(result);
        }

        // Allow special cases for constraints that behave differently when request doesn't contain expected key.
        protected virtual void ConfirmRequestValueNonMatch(
            ActionConstraintContext context,
            Func<ActionConstraintContext, bool> process)
        {
            // Act
            var result = process(context);

            // Assert
            ConfirmRequestValueNonMatch(context, result);
        }

        protected virtual ActionConstraintContext GetContext(IActionConstraint constraint)
        {
            return new ActionConstraintContext
            {
                CurrentCandidate = new ActionSelectorCandidate(new ActionDescriptor(), new[] { constraint }),
                RouteContext = new RouteContext(new DefaultHttpContext()),
            };
        }
    }
}
