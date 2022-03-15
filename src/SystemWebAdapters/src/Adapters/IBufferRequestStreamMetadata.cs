// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web.Adapters;

public interface IBufferRequestStreamMetadata
{
    bool IsEnabled { get; }

    int BufferThreshold { get; }

    long? BufferLimit { get; }
}
