// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.FxCop.Sdk;

namespace Microsoft.Web.FxCop
{
    public static class TypeNodeExtensions
    {
        private const string TaskPortableAssembly = "System.Threading.Tasks";
        private const string TaskNamespace = "System.Threading.Tasks";
        private const string TaskType = "Task";
        private const string TaskGenericType = "Task`1";
        private static readonly object _taskLock = new object();

        private static TypeNode _task;
        private static TypeNode _taskGeneric;

        public static bool IsTask(this TypeNode type)
        {
            EnsureTaskTypesInitialized(type);
            return IsTaskCore(type);
        }

        private static bool IsTaskCore(TypeNode type)
        {
            Contract.Assert(_task != null);
            Contract.Assert(_taskGeneric != null);

            if (type.Name.UniqueIdKey == _task.Name.UniqueIdKey)
            {
                return true;
            }

            if (!type.IsGeneric)
            {
                return false;
            }

            TypeNode targetType = _taskGeneric.GetGenericTemplateInstance(_taskGeneric.DeclaringModule, type.ConsolidatedTemplateArguments);
            return targetType.Name.UniqueIdKey == type.Name.UniqueIdKey;
        }

        private static void EnsureTaskTypesInitialized(TypeNode type)
        {
            lock (_taskLock)
            {
                if (_task == null)
                {
                    Contract.Assert(_task == null);
                    Contract.Assert(_taskGeneric == null);

                    AssemblyNode taskAssembly = GetTaskAssembly(type);
                    Contract.Assert(taskAssembly != null);

                    _task = GetTypeNode(taskAssembly, TaskNamespace, TaskType);
                    _taskGeneric = GetTypeNode(taskAssembly, TaskNamespace, TaskGenericType);

                    Contract.Assert(_task != null);
                    Contract.Assert(_taskGeneric != null);
                }
            }
        }

        private static TypeNode GetTypeNode(AssemblyNode assembly, string ns, string name)
        {
            Contract.Assert(assembly != null);

            return assembly.GetType(Identifier.For(ns), Identifier.For(name));
        }

        private static AssemblyNode GetTaskAssembly(TypeNode type)
        {
            AssemblyNode taskAssembly = null;

            // Check if the type's located in mscorlib
            TypeNode taskTypeNode = GetTypeNode(FrameworkAssemblies.Mscorlib, TaskNamespace, TaskGenericType);
            if (taskTypeNode != null)
            {
                taskAssembly = FrameworkAssemblies.Mscorlib;
            }
            else if (type.DeclaringModule.Name.Equals(TaskPortableAssembly))
            {
                // If the type is a type in the portable assembly, no need to loop through the assembly references
                taskAssembly = type.DeclaringModule.ContainingAssembly;
            }
            else
            {
                AssemblyReference assemblyReference = type.DeclaringModule
                                                          .AssemblyReferences
                                                          .FirstOrDefault(reference => reference.Name.Equals(TaskPortableAssembly, StringComparison.Ordinal));
                Contract.Assert(assemblyReference != null);
                taskAssembly = assemblyReference.Assembly;
            }

            Contract.Assert(taskAssembly != null);
            return taskAssembly;
        }
    }
}
