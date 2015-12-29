// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.WebHooks.Properties;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Describes the contents and behavior or a WebHook. A <see cref="WebHook"/> is similar to a subscription in a 
    /// pub/sub system in that it allows the subscriber to indicate when and how event notifications should get 
    /// dispatched and where they should get dispatched to. A <see cref="WebHook"/> is registered and managed on a 
    /// per user basis which means that each user has a separate set of WebHooks that can get trigged by actions
    /// executed by that user. That is, user <c>A</c> will not see a WebHook fired for an action performed by user <c>B</c>.
    /// </summary>
    public class WebHook
    {
        private readonly HashSet<string> _filters;
        private readonly IDictionary<string, string> _headers;
        private readonly IDictionary<string, object> _properties;
        private string id;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHook"/> class.
        /// </summary>
        public WebHook()
        {
            id = GetId();
            _filters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets a unique ID of the WebHook. This ID can be used to later refer to the WebHook in case it
        /// needs to be updated or deleted. The ID is by default in the form of a <see cref="Guid"/> and if the field 
        /// is cleared it will be reset to a <see cref="Guid"/>.
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                id = string.IsNullOrEmpty(value) ? GetId() : value;
            }
        }

        /// <summary>
        /// Gets or sets the URI of the WebHook.
        /// </summary>
        [Required]
        public Uri WebHookUri { get; set; }

        /// <summary>
        /// Gets or sets the secret used to sign the body of the WebHook request.
        /// </summary>
        [Required, StringLength(maximumLength: 64, MinimumLength = 32, ErrorMessageResourceType = typeof(CustomResources), ErrorMessageResourceName = "WebHook_InvalidSecret")]
        public string Secret { get; set; }

        /// <summary>
        /// Gets or sets a description of the WebHook.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the WebHook is paused.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Gets the set of case-insensitive filters associated with this WebHook. The filters indicate 
        /// which WebHook events that this WebHook will be notified for. The list of filters can be obtained from
        /// the registered <see cref="IWebHookFilterManager"/> instance.
        /// </summary>
        public ISet<string> Filters
        {
            get
            {
                return _filters;
            }
        }

        /// <summary>
        /// Gets a set of additional HTTP headers that will be sent with the WebHook request.
        /// </summary>
        public IDictionary<string, string> Headers
        {
            get
            {
                return _headers;
            }
        }

        /// <summary>
        /// Gets a set of additional case-insensitive properties that will be sent with the WebHook request
        /// as part of the HTTP request entity body.
        /// </summary>
        public IDictionary<string, object> Properties
        {
            get
            {
                return _properties;
            }
        }

        private static string GetId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
