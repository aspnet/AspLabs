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
    // Variant of WebHookConstraintTestBase which eliminates most tests using ActionDescriptor.RouteValues.
    public abstract class WebHookEventNamesConstraintTestBase
    {
        [Fact]
        public void Accept_Fails_OnEmpty()
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(WebHookConstraintTestBase.MatchingDataSet), MemberType = typeof(WebHookConstraintTestBase))]
        public void Accept_Succeeds_OnRouteContextMatch(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(WebHookConstants.EventKeyName, requestValue);

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Accept_Succeeds_OnRouteContextMatch_WithMultipleNames()
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add($"{WebHookConstants.EventKeyName}[0]", "another1");
            context.RouteContext.RouteData.Values.Add($"{WebHookConstants.EventKeyName}[1]", "nonMatch");
            context.RouteContext.RouteData.Values.Add($"{WebHookConstants.EventKeyName}[2]", "match");
            context.RouteContext.RouteData.Values.Add($"{WebHookConstants.EventKeyName}[3]", "another2");

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [MemberData(nameof(WebHookConstraintTestBase.NonMatchingDataSet), MemberType = typeof(WebHookConstraintTestBase))]
        public void Accept_Fails_OnRouteContextNonMatch(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(WebHookConstants.EventKeyName, requestValue);

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(WebHookConstraintTestBase.NoRequestValueDataSet), MemberType = typeof(WebHookConstraintTestBase))]
        public void Accept_Fails_OnNoRouteContextRequestValue(string requestValue)
        {
            // Arrange
            var constraint = GetConstraint();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(WebHookConstants.EventKeyName, requestValue);

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.False(result);
        }

        // Constraint does not check ActionDescriptor for event names.
        [Fact]
        public void Accept_Fails_OnActionDescriptorMatch()
        {
            // Arrange
            var constraint = GetConstraintForPingMatch();
            var context = GetContext(constraint);
            context.CurrentCandidate.Action.RouteValues.Add(WebHookConstants.EventKeyName, "match");

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.False(result);
        }

        // Constraint does not check ActionDescriptor for event names.
        [Fact]
        public void Accept_Fails_OnActionDescriptorPingMatch()
        {
            // Arrange
            var constraint = GetConstraintForPingMatch();
            var context = GetContext(constraint);
            context.CurrentCandidate.Action.RouteValues.Add(WebHookConstants.EventKeyName, "pingMatch");

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Accept_Succeeds_OnRouteContextPingMatch()
        {
            // Arrange
            var constraint = GetConstraintForPingMatch();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add(WebHookConstants.EventKeyName, "pingMatch");

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Accept_Succeeds_OnRouteContextPingMatch_WithMultipleNames()
        {
            // Arrange
            var constraint = GetConstraintForPingMatch();
            var context = GetContext(constraint);
            context.RouteContext.RouteData.Values.Add($"{WebHookConstants.EventKeyName}[0]", "another1");
            context.RouteContext.RouteData.Values.Add($"{WebHookConstants.EventKeyName}[1]", "nonMatch");
            context.RouteContext.RouteData.Values.Add($"{WebHookConstants.EventKeyName}[2]", "pingMatch");
            context.RouteContext.RouteData.Values.Add($"{WebHookConstants.EventKeyName}[3]", "another2");

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Accept_Fails_OnRouteContextPingMatch_IfSecond()
        {
            // Arrange
            var constraint = GetConstraintForPingMatch();

            // E.g. the other action is configured for a different event using the same receiver-specific attribute.
            var firstConstraint = GetConstraintForPingMatch();

            var context = GetContext(constraint);
            context.Candidates = new[]
            {
                new ActionSelectorCandidate(new ActionDescriptor(), new[] { firstConstraint }),
                context.CurrentCandidate,
            };

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Accept_Fails_OnRouteContextPingMatch_IfLaterActionLacksConstraint()
        {
            // Arrange
            var constraint = GetConstraintForPingMatch();
            var context = GetContext(constraint);
            context.CurrentCandidate.Action.RouteValues.Add(WebHookConstants.EventKeyName, "pingMatch");
            context.Candidates = new[]
            {
                context.CurrentCandidate,
                new ActionSelectorCandidate(new ActionDescriptor(), Array.Empty<IActionConstraint>()),
            };

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Accept_Fails_OnRouteContextPingMatch_IfLaterActionWillMatch()
        {
            // Arrange
            var constraint = GetConstraintForPingMatch();

            // E.g. the other action is configured specifically for ping requests.
            var secondConstraint = new WebHookSingleEventNamesConstraint("pingMatch", pingEventName: "pingMatch");

            var context = GetContext(constraint);
            context.CurrentCandidate.Action.RouteValues.Add(WebHookConstants.EventKeyName, "pingMatch");
            context.Candidates = new[]
            {
                context.CurrentCandidate,
                new ActionSelectorCandidate(new ActionDescriptor(), new[] { secondConstraint }),
            };

            // Act
            var result = constraint.Accept(context);

            // Assert
            Assert.False(result);
        }

        protected abstract IActionConstraint GetConstraint();

        protected abstract IActionConstraint GetConstraintForPingMatch();

        protected virtual ActionConstraintContext GetContext(IActionConstraint constraint)
        {
            var candidate = new ActionSelectorCandidate(new ActionDescriptor(), new[] { constraint });
            var context = new ActionConstraintContext
            {
                Candidates = new[] { candidate },
                CurrentCandidate = candidate,
                RouteContext = new RouteContext(new DefaultHttpContext()),
            };

            return context;
        }
    }
}
