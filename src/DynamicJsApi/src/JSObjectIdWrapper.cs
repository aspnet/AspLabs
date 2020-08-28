// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.DynamicJS
{
    internal struct JSObjectIdWrapper
    {
        [JsonPropertyName("__objectId")]
        public long Id { get; set; }
    }
}
