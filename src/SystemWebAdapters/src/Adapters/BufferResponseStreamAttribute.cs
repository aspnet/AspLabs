// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class BufferResponseStreamAttribute : Attribute, IBufferResponseStreamMetadata
{
    public bool IsEnabled { get; set; } = true;

    public int MemoryThreshold { get; set; } = 32768; // Same default as FileBufferingWriteStream

    public long? BufferLimit { get; set; }
}
