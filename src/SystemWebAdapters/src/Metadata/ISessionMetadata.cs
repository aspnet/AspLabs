// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Metadata;

public interface ISessionMetadata
{
    bool IsEnabled { get; }

    bool IsReadOnly { get; }
}
