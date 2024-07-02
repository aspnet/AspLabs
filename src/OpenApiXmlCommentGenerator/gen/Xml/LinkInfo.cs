// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.OpenApi.SourceGenerators;

public sealed class LinkInfo(LinkType linkType, string linkId, string? commentId, string? altText)
{
    public LinkType LinkType { get; init; } = linkType;
    public string LinkId { get; init; } = linkId;
    public string? CommentId { get; init; } = commentId;
    public string? AltText { get; init; } = altText;

    internal LinkInfo Clone()
    {
        return (LinkInfo)MemberwiseClone();
    }
}

public enum LinkType
{
    CRef,
    HRef,
}
