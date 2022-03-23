// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO;

namespace System.Web.Adapters;

internal interface IBufferedResponseFeature
{
    bool IsEnded { get; set; }

    bool SuppressContent { get; set; }

    Stream Stream { get; }

    void ClearContent();
}
