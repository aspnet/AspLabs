// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Description of where a <see cref="WebHookParameter"/> value is found and how
    /// <see cref="WebHookParameter.SourceName"/> should be interpreted.
    /// </summary>
    /// <remarks>Starts at <c>1</c> to ensure <see cref="WebHookParameterType"/> instances are initialized.</remarks>
    public enum WebHookParameterType
    {
        /// <summary>
        /// <see cref="WebHookParameter"/> value is found in the request headers. That is,
        /// <see cref="WebHookParameter.SourceName"/> is a header name.
        /// </summary>
        Header = 1,

        /// <summary>
        /// <see cref="WebHookParameter"/> value is found in the <see cref="AspNetCore.Routing.RouteValueDictionary"/>.
        /// That is, <see cref="WebHookParameter.SourceName"/> is a
        /// <see cref="AspNetCore.Routing.RouteValueDictionary"/> key name.
        /// </summary>
        RouteValue,

        /// <summary>
        /// <see cref="WebHookParameter"/> value is found in the query string. That is,
        /// <see cref="WebHookParameter.SourceName"/> is a query parameter name.
        /// </summary>
        QueryParameter,
    }
}
