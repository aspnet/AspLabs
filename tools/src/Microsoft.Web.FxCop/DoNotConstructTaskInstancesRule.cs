// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.FxCop.Sdk;

namespace Microsoft.Web.FxCop
{
    public class DoNotConstructTaskInstancesRule : IntrospectionRule
    {
        public DoNotConstructTaskInstancesRule()
            : base("DoNotConstructTaskInstances")
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

        public override void VisitConstruct(Construct construct)
        {
            var memberBinding = construct.Constructor as MemberBinding;

            if (memberBinding != null
                && memberBinding.BoundMember.Name.Name == ".ctor"
                && memberBinding.BoundMember.DeclaringType.IsTask())
            {
                Problems.Add(new Problem(GetResolution(), construct.UniqueKey.ToString()));
            }

            base.VisitConstruct(construct);
        }
    }
}
