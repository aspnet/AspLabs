// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DocFx.XmlComments;

public class SourceDetail
{
    public GitDetail? Remote { get; set; }

    public string? Name { get; set; }

    /// <summary>
    /// The url path for current source, should be resolved at some late stage
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// The local path for current source, should be resolved to be relative path at some late stage
    /// </summary>
    public string? Path { get; set; }

    public int StartLine { get; set; }

    public int EndLine { get; set; }
}
