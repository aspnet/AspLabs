// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Describes change of specific field
    /// </summary>
    /// <typeparam name="T">The string-type of the field that is being changed</typeparam>
    public class WorkItemUpdatedFieldValue<T>
    {
        /// <summary>
        /// Gets the value of the field before the change.
        /// </summary>
        [JsonProperty("oldValue")]
        public T OldValue { get; set; }

        /// <summary>
        /// Gets the value of the field after the change.
        /// </summary>
        [JsonProperty("newValue")]
        public T NewValue { get; set; }
    }
}
