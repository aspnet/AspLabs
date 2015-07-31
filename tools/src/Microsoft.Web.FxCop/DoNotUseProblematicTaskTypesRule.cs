// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.FxCop.Sdk;

namespace Microsoft.Web.FxCop
{
    public class DoNotUseProblematicTaskTypesRule : IntrospectionRule
    {
        private readonly Dictionary<string, string> _problematicTypes = GetProblematicTypes();

        public DoNotUseProblematicTaskTypesRule()
            : base("DoNotUseProblematicTaskTypes")
        {
        }

        public override ProblemCollection Check(Member member)
        {
            var method = member as Method;
            if (method != null)
            {
                VisitStatements(method.Body.Statements);
            }

            return Problems;
        }

        public override void VisitMemberBinding(MemberBinding memberBinding)
        {
            var method = memberBinding.BoundMember as Method;
            if (method != null)
            {
                string message;
                if (_problematicTypes.TryGetValue(method.DeclaringType.FullName, out message))
                {
                    Problems.Add(new Problem(GetResolution(method.DeclaringType.FullName, message), memberBinding.UniqueKey.ToString()));
                }
            }

            base.VisitMemberBinding(memberBinding);
        }

        private static Dictionary<string, string> GetProblematicTypes()
        {
            return new Dictionary<string, string>
            {
                { "System.Threading.Tasks.Parallel", "The methods on this type are blocking operations." },
                { "System.Threading.Tasks.TaskExtensions", "The .Unwrap() method does not have good performance characteristics. Use the .FastUnwrap() extension method instead." },
                { "System.Threading.Tasks.TaskFactory", "If you need to create a Task, use the TaskHelpers class instead." },
                { "System.Threading.Tasks.TaskScheduler", "If you need to create a Task, use the TaskHelpers class instead." }
            };
        }
    }
}
