// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.FxCop.Sdk;

namespace Microsoft.Web.FxCop
{
    public class DoNotCallProblematicMethodsOnTaskRule : IntrospectionRule
    {
        private readonly Dictionary<string, string> _problematicMethods = GetProblematicMethods();

        public DoNotCallProblematicMethodsOnTaskRule()
            : base("DoNotCallProblematicMethodsOnTask")
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
                if (_problematicMethods.TryGetValue(method.Name.Name, out message) &&
                    method.DeclaringType.IsTask())
                {
                    Problems.Add(new Problem(GetResolution(method.Name.Name, message), memberBinding.UniqueKey.ToString()));
                }
            }

            base.VisitMemberBinding(memberBinding);
        }

        private static Dictionary<string, string> GetProblematicMethods()
        {
            return new Dictionary<string, string>
            {
                { "get_Result", "Calls to this method are difficult to get correct or do not have good performance characteristics. Use the .Then(), .Catch(), or .Finally() extension method instead." },
                { "get_Factory", "If you need to create a Task, use the TaskHelpers class instead." },
                { "ContinueWith", "Calls to this method are difficult to get correct or do not have good performance characteristics. Use the .Then(), .Catch(), or .Finally() extension method instead." },
                { "Dispose", "Tasks should never be disposed of." },
                { "Run", "If you need to create a Task, use the TaskHelpers class instead." },
                { "RunSynchronously", "If you need to create a Task, use the TaskHelpers class instead." },
                { "Start", "If you need to create a Task, use the TaskHelpers class instead." },
                { "Wait", "This is a blocking call. Switch to an asynchronous call like .Then() instead." },
                { "WaitAll", "This is a blocking call. Switch to an asynchronous call like .Then() instead." },
                { "WaitAny", "This is a blocking call. Switch to an asynchronous call like .Then() instead." },
                { "Yield", "This call forces a thread transition which can hurt server performance. Do not use." },
            };
        }
    }
}
