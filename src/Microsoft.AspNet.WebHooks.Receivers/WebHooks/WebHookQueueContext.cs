// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.WebHooks
{
    /// <summary>
    /// Provides a queue context used by <see cref="WebHookQueueHandler" /> to enqueue a WebHook for subsequent
    /// processing.
    /// </summary>
    public class WebHookQueueContext
    {
        private List<string> _actions;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookQueueContext"/> with default values.
        /// </summary>
        public WebHookQueueContext()
        {
            _actions = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookQueueContext"/> with the given <paramref name="context"/>.
        /// </summary>
        /// <param name="receiver">The name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.</param>
        /// <param name="context">The <see cref="WebHookHandlerContext"/> instance for this WebHook.</param>
        public WebHookQueueContext(string receiver, WebHookHandlerContext context)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException(nameof(receiver));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Receiver = receiver;
            Id = context.Id;
            Data = context.Data;
            _actions = context.Actions.ToList();
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="IWebHookReceiver"/> which processed the incoming WebHook. The
        /// receiver can for example be <c>dropbox</c> or <c>github</c>.
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// Gets or sets a (potentially empty) ID of a particular configuration for this WebHook. This ID can be 
        /// used to differentiate between WebHooks from multiple senders registered with the same receiver.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the set of actions that caused the WebHook to be fired.
        /// </summary>
        public ICollection<string> Actions
        {
            get
            {
                return _actions;
            }
        }

        /// <summary>
        /// Gets or sets the optional data associated with this WebHook. The data typically represents the
        /// HTTP request entity body of the incoming WebHook but can have been processed in various ways
        /// by the corresponding <see cref="IWebHookReceiver"/>.
        /// </summary>
        public object Data { get; set; }
    }
}