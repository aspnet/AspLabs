// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SessionAttribute : Attribute, ISessionMetadata
{
    public SessionBehavior Behavior { get; set; } = SessionBehavior.PreLoad;

    public bool IsReadOnly { get; set; } = true;
}
