// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters;

public interface IBufferResponseStreamMetadata
{
    bool IsEnabled { get; }

    int MemoryThreshold { get; }

    long? BufferLimit { get; }
}
