// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.AspNetCore.SystemWebAdapters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PreBufferRequestStreamAttribute : Attribute, IPreBufferRequestStreamMetadata
{
    // Same limit as the default: https://source.dot.net/#Microsoft.AspNetCore.Http/Internal/BufferingHelper.cs,47b7015acb14f2a4
    private const int DefaultBufferThreshold = 1024 * 30;

    public bool IsEnabled { get; set; } = true;

    public int BufferThreshold { get; set; } = DefaultBufferThreshold;

    public long? BufferLimit { get; set; }
}
