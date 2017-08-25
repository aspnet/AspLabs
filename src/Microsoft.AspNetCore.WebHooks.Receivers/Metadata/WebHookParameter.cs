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
        /// Initializes a new <see cref="WebHookParameter"/> with the given <paramref name="name"/> and
        /// <paramref name="headerName"/>. <see cref="IsQueryParameter"/> and <see cref="IsRequired"/> are
        /// <see langword="false"/> when using this constructor.
        /// </summary>
        /// <param name="name">The name of an action parameter.</param>
        /// <param name="headerName">The name of the HTTP header containing this parameter's value.</param>
        public WebHookParameter(string name, string headerName)
            : this(name, sourceName: headerName, isQueryParameter: false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="WebHookParameter"/> with the given <paramref name="name"/>,
        /// <paramref name="sourceName"/> and <paramref name="isQueryParameter"/>. <see cref="IsRequired"/> is
        /// <see langword="false"/> when using this constructor.
        /// </summary>
        /// <param name="name">The name of an action parameter.</param>
        /// <param name="sourceName">
        /// The name of the HTTP header or, if <paramref name="isQueryParameter"/> is <see langword="true"/>, query
        /// parameter containing this parameter's value.
        /// </param>
        /// <param name="isQueryParameter">
        /// Specifies whether <paramref name="sourceName"/> refers to a query parameter. If <see langword="true"/>,
        /// <paramref name="sourceName"/> refers to a query parameter. Otherwise, <paramref name="sourceName"/> refers
        /// to an HTTP header.
        /// </param>
        public WebHookParameter(string name, string sourceName, bool isQueryParameter)
            : this(name, sourceName, isQueryParameter, isRequired: false)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="WebHookParameter"/> with the given <paramref name="name"/>,
        /// <paramref name="sourceName"/>, <paramref name="isQueryParameter"/>, and <paramref name="isRequired"/>.
        /// </summary>
        /// <param name="name">The name of an action parameter.</param>
        /// <param name="sourceName">
        /// The name of the HTTP header or, if <paramref name="isQueryParameter"/> is <see langword="true"/> query
        /// parameter containing this parameter's value.
        /// </param>
        /// <param name="isQueryParameter">
        /// Specifies whether <paramref name="sourceName"/> refers to a query parameter. If <see langword="true"/>,
        /// <paramref name="sourceName"/> refers to a query parameter. Otherwise, <paramref name="sourceName"/> refers
        /// to an HTTP header.
        /// </param>
        /// <param name="isRequired">
        /// Specifies whether the <paramref name="sourceName"/> HTTP header or, if <paramref name="isQueryParameter"/>
        /// is <see langword="true"/>, query parameter is required in a WebHook request. If <see langword="true"/> and
        /// the header or query parameter is missing, the receiver will respond with status code 400 "Bad Request".
        /// </param>
        public WebHookParameter(string name, string sourceName, bool isQueryParameter, bool isRequired)
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
            SourceName = sourceName;
            IsQueryParameter = isQueryParameter;
            IsRequired = isRequired;
        }

        /// <summary>
        /// Gets the name of an action parameter.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name of the HTTP header or, if <see cref="IsQueryParameter"/> is <see langword="true"/>, query
        /// parameter containing this parameter's value.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Gets an indication <see cref="SourceName"/> refers to a query parameter.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, <see cref="SourceName"/> refers to a query parameter. Otherwise,
        /// <see cref="SourceName"/> refers to an HTTP header.
        /// </value>
        public bool IsQueryParameter { get; }

        /// <summary>
        /// Gets an indication the <see cref="SourceName"/> HTTP header or, if <see cref="IsQueryParameter"/> is
        /// <see langword="true"/>, query parameter is required in a WebHook request.
        /// </summary>
        /// <value>
        /// If <see langword="true"/> and the header or query parameter is missing, the receiver will respond with
        /// status code 400 "Bad Request". Otherwise, no additional validation is performed.
        /// </value>
        public bool IsRequired { get; }
    }
}
