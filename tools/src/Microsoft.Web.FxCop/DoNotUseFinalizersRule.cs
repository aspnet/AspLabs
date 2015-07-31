// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.FxCop.Sdk;

namespace Microsoft.Web.FxCop
{
    public class DoNotUseFinalizersRule : IntrospectionRule
    {
        public DoNotUseFinalizersRule()
            : base("DoNotUseFinalizers")
        {
        }

        public override ProblemCollection Check(Member member)
        {
            if (member.NodeType == NodeType.Method && member.Name.Name == "Finalize")
            {
                Problems.Add(new Problem(GetResolution(member.DeclaringType.FullName), member));
            }

            return Problems;
        }
    }
}
