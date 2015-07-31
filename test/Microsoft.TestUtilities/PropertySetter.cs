// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.TestUtilities
{
    /// <summary>
    /// Determines how the property under test is expected to behave.
    /// </summary>
    public enum PropertySetter
    {
        /// <summary>
        /// It is not allowed to set the property to null
        /// </summary>
        NullThrows = 0,

        /// <summary>
        /// Setting the property to null causes it to get reinitialized to some non-null value
        /// </summary>
        NullSetsDefault,

        /// <summary>
        /// Null round-trips like any other value
        /// </summary>
        NullRoundtrips,

        /// <summary>
        /// Null does not throw and does not round-trip but sets a non-deterministic non-null value.
        /// This can for example be a GUID or a timestamp.
        /// </summary>
        NullDoesNotRoundtrip
    }
}
