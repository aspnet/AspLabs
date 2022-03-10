// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SystemWebAdapterAttribute : Attribute
    {
        public bool Enabled { get; set; } = true;

        public bool BufferRequestStream { get; set; } = true;
    }
}
