// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.WebHooks.Routing
{
    /// <summary>
    /// A base class for <see cref="IActionConstraint"/> implementations which use WebHook event names to select
    /// candidate actions.
    /// </summary>
    public abstract class WebHookEventNamesConstraint : IActionConstraint
    {
        /// <summary>
        /// Instantiates a new <see cref="WebHookEventNamesConstraint"/> instance.
        /// </summary>
        protected WebHookEventNamesConstraint()
        {
        }

        // Running this constraint last avoids NotFound responses to ping requests because other actions have different
        // constraints on them.
        /// <inheritdoc />
        /// <value>Chosen to ensure constraints of this type run last.</value>
        public int Order => int.MaxValue;

        /// <inheritdoc />
        public abstract bool Accept(ActionConstraintContext context);

        /// <summary>
        /// Gets an indication the action is a valid candidate for selection.
        /// </summary>
        /// <param name="context">The <see cref="ActionConstraintContext"/>.</param>
        /// <param name="eventName">The event name to match.</param>
        /// <param name="pingEventName">Name of the ping event this action expects, if any.</param>
        /// <returns>
        /// <see langword="true"/> if the action is valid for selection; <see langword="false"/> otherwise.
        /// </returns>
        protected bool Accept(ActionConstraintContext context, string eventName, string pingEventName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.RouteContext.RouteData.TryGetWebHookEventNames(out var eventNames))
            {
                if (eventNames.Any(name => string.Equals(eventName, name, StringComparison.OrdinalIgnoreCase)))
                {
                    // Simple case. Request is for the expected event.
                    return true;
                }

                if (pingEventName != null &&
                    eventNames.Any(name => string.Equals(pingEventName, name, StringComparison.OrdinalIgnoreCase)))
                {
                    // Decide if this is the candidate that should support the ping event. Ping events must succeed
                    // even if all actions are for other event names.
                    // 1. Is this the first candidate? If not, never handle ping events.
                    if (context.Candidates[0].Action != context.CurrentCandidate.Action)
                    {
                        return false;
                    }

                    // 2. Does another candidate lack this constraint? If yes, they'll handle ping events.
                    for (var i = 1; i < context.Candidates.Count; i++)
                    {
                        var candidate = context.Candidates[i];
                        if (candidate.Constraints == null ||
                            !candidate.Constraints.Any(constraint => constraint is WebHookEventNamesConstraint))
                        {
                            return false;
                        }
                    }

                    // 3. Is another candidate configured to accept this request?
                    for (var i = 1; i < context.Candidates.Count; i++)
                    {
                        var candidate = context.Candidates[i];

                        // ??? Any better to choose all constraints that have Order==int.MexValue?
                        var constraints = context.Candidates.OfType<WebHookEventNamesConstraint>();
                        var innerContext = new ActionConstraintContext
                        {
                            Candidates = context.Candidates,
                            CurrentCandidate = candidate,
                            RouteContext = context.RouteContext,
                        };

                        // Constraints shouldn't actually contain more than one instance. But, doesn't hurt to check.
                        if (constraints.All(constraint => constraint.Accept(innerContext)))
                        {
                            return false;
                        }
                    }

                    // 4. No other candidate accepts ping events. It's up to us.
                    return true;
                }
            }

            return false;
       }
    }
}
