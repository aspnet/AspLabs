// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// A work item represents the act of firing a WebHook.
    /// </summary>
    internal class WebHookWorkItem
    {
        private Collection<string> _actions = new Collection<string>();
        private IDictionary<string, object> _data = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a unique ID which is used to identify this firing of a <see cref="WebHook"/>.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WebHook"/> to fire.
        /// </summary>
        public WebHook Hook { get; set; }

        /// <summary>
        /// Gets or sets the offset (starting with zero) identifying the launch line to be used when firing.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Sets the set of actions that caused this <see cref="WebHook"/> to fire.
        /// </summary>
        public Collection<string> Actions
        {
            get
            {
                return _actions;
            }
        }

        /// <summary>
        /// Sets the extra data that should be submitted as part of this <see cref="WebHook"/> request.
        /// </summary>
        public IDictionary<string, object> Data
        {
            get
            {
                return _data;
            }
        }
    }
}
