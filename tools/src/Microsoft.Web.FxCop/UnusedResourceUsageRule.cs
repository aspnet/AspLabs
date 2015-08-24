// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FxCop.Sdk;

namespace Microsoft.Web.FxCop
{
    public class UnusedResourceUsageRule : IntrospectionRule
    {
        private static readonly object _lock = new object();
        private static readonly IDictionary<AssemblyNode, ISet<PropertyNode>> _availableResources = new ConcurrentDictionary<AssemblyNode, ISet<PropertyNode>>();
        private static readonly IDictionary<AssemblyNode, ISet<PropertyNode>> _usedResources = new ConcurrentDictionary<AssemblyNode, ISet<PropertyNode>>();

        public UnusedResourceUsageRule()
            : base("UnusedResourceUsageRule")
        {
        }

        public override TargetVisibilities TargetVisibility
        {
            get { return TargetVisibilities.All; }
        }

        public override ProblemCollection Check(ModuleNode node)
        {
            var assemblyNode = node as AssemblyNode;
            if (assemblyNode != null)
            {
                var assemblyAvailableResources = new HashSet<PropertyNode>();
                var assemblyUsedResources = new HashSet<PropertyNode>();

                _availableResources[assemblyNode] = assemblyAvailableResources;
                _usedResources[assemblyNode] = assemblyUsedResources;

                VisitAssembly(assemblyNode);

                IEnumerable<PropertyNode> unusedResources = from res in assemblyAvailableResources.Except(assemblyUsedResources)
                                                            where !IsCommonResource(res)
                                                            select res;

                foreach (PropertyNode item in unusedResources)
                {
                    Problems.Add(new Problem(this.GetResolution(item.Name.Name, item.DeclaringType.FullName), item.UniqueKey.ToString()));
                }
            }

            return Problems;
        }

        public override void VisitProperty(PropertyNode property)
        {
            if (IsResourceType(property.DeclaringType) && IsResource(property))
            {
                AddItemWithLock(_availableResources[(AssemblyNode)property.DeclaringType.DeclaringModule], property);
            }

            base.VisitProperty(property);
        }

        public override void VisitMethodCall(MethodCall call)
        {
            MemberBinding mb = call.Callee as MemberBinding;
            if (mb != null)
            {
                Method methodBeingCalled = mb.BoundMember as Method;
                if (methodBeingCalled != null
                        && IsResourceType(methodBeingCalled.DeclaringType)
                        && IsResource(methodBeingCalled.DeclaringMember as PropertyNode))
                {

                    var property = methodBeingCalled.DeclaringMember as PropertyNode;

                    ISet<PropertyNode> properties = null;
                    // Look up the assembly from the dictionary. If the assembly could not be looked up, the resource must have been declared outside the current assembly
                    if (_usedResources.TryGetValue((AssemblyNode)property.DeclaringType.DeclaringModule, out properties))
                    {
                        AddItemWithLock(properties, property);
                    }
                }
            }

            base.VisitMethodCall(call);
        }

        private static void AddItemWithLock<T>(ISet<T> item, T value)
        {
            lock (_lock)
            {
                item.Add(value);
            }
        }

        private static bool IsResource(PropertyNode property)
        {
            return property != null && property.Type.FullName.Equals(typeof(System.String).FullName);
        }

        private static bool IsResourceType(TypeNode typeNode)
        {
            var classNode = typeNode as ClassNode;
            return (classNode != null
                && classNode.Attributes.Any(c => c.Type.FullName.Equals(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).FullName))
                && classNode.Name.Name.Contains("Resource"));
        }

        /// <summary>
        /// Hack to determine if the file is "Common*Resources.resx" that is shared amongst multiple WebHooks projects. 
        /// Strings in this resx are ignored since they may have dependencies outside the current assembly.
        /// </summary>
        private static bool IsCommonResource(PropertyNode property)
        {
            string name = property.DeclaringType.Name.Name;
            return name.StartsWith("Common") && name.EndsWith("Resources");
        }
    }
}

