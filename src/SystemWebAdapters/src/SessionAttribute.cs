// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web.Metadata;

namespace System.Web;

public class SessionAttribute : ISessionMetadata
{
    public bool IsEnabled { get; set; } = true;

    public bool IsReadOnly { get; set; } = true;
}
