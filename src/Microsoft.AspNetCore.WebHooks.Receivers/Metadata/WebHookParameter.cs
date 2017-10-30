// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.WebHooks.Properties;

namespace Microsoft.AspNetCore.WebHooks.Metadata
{
    /// <summary>
    /// Metadata describing an additional parameter supported in a receiver.
    /// </summary>
    public class WebHookParameter
    {
        /// <summary>
        /// Initializes a new <see cref="WebHookParameter"/> with the given <paramref name="name"/>,
        /// <paramref name="parameterType"/> and <paramref name="sourceName"/>. <see cref="IsRequired"/> is
        /// <see langword="false"/> when using this constructor.
        /// </summary>
        /// <param name="name">The name of an action parameter.</param>
        /// <param name="parameterType">The <see cref="WebHookParameterType"/> of this parameter.</param>
        /// <param name="sourceName">
        /// The name of the HTTP header, <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry or query parameter
        /// containing this parameter's value.
        /// </param>
        public WebHookParameter(string name, WebHookParameterType parameterType, string sourceName)
            : this(name, parameterType, sourceName, isRequired: false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="WebHookParameter"/> with the given <paramref name="name"/>,
        /// <paramref name="parameterType"/>, <paramref name="sourceName"/>, and <paramref name="isRequired"/>.
        /// </summary>
        /// <param name="name">The name of an action parameter.</param>
        /// <param name="parameterType">The <see cref="WebHookParameterType"/> of this parameter.</param>
        /// <param name="sourceName">
        /// The name of the HTTP header, <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry or query parameter
        /// containing this parameter's value.
        /// </param>
        /// <param name="isRequired">
        /// Specifies whether the <see cref="SourceName"/> HTTP header,
        /// <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry or query parameter is required in a WebHook
        /// request.
        /// </param>
        public WebHookParameter(string name, WebHookParameterType parameterType, string sourceName, bool isRequired)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new System.ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(name));
            }
            if (string.IsNullOrEmpty(sourceName))
            {
                throw new System.ArgumentException(Resources.General_ArgumentCannotBeNullOrEmpty, nameof(sourceName));
            }

            Name = name;
            ParameterType = parameterType;
            SourceName = sourceName;
            IsRequired = isRequired;
        }

        /// <summary>
        /// Gets the name of an action parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="WebHookParameterType"/> of this parameter.
        /// </summary>
        public WebHookParameterType ParameterType { get; }

        /// <summary>
        /// Gets the name of the HTTP header, <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry or query
        /// parameter containing this parameter's value.
        /// </summary>
        /// <seealso cref="ParameterType"/>
        public string SourceName { get; }

        /// <summary>
        /// Gets an indication the <see cref="SourceName"/> HTTP header,
        /// <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry or query parameter is required in a WebHook
        /// request.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> and the <see cref="SourceName"/> HTTP header
        /// <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry or query parameter is missing, the receiver
        /// will respond with status code 400 "Bad Request". Otherwise, no additional validation is performed.
        /// </value>
        public bool IsRequired { get; }
    }
}
