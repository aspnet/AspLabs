// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace System.Runtime.CompilerServices;

#if !NET5_0_OR_GREATER

// this class is needed for init properties.
// It was added to .NET 5.0 but for earlier versions we need to specify it manually
internal static class IsExternalInit { }
#endif

#if !NET7_0_OR_GREATER
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
internal sealed class RequiredMemberAttribute : Attribute {}

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
internal sealed class CompilerFeatureRequiredAttribute : Attribute
{
    public CompilerFeatureRequiredAttribute(string featureName)
    {
        FeatureName = featureName;
    }

    public string FeatureName { get; }
    public bool   IsOptional  { get; init; }

    public const string RefStructs      = nameof(RefStructs);
    public const string RequiredMembers = nameof(RequiredMembers);
}

#endif // !NET7_0_OR_GREATER

#if !NET7_0_OR_GREATER
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute {}
#endif
