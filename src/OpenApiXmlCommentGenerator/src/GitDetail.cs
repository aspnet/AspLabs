// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DocFx.XmlComments;

public record GitDetail
{
    /// <summary>
    /// Relative path of current file to the Git Root Directory
    /// </summary>
    public string? Path { get; set; }
    public string? Branch { get; set; }
    public string? Repo { get; set; }
}
