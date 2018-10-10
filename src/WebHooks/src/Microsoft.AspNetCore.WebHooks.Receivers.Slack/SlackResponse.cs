// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// An Slack WebHook action can post back a response to a Slack channel by returning a <see cref="SlackResponse"/>
    /// or an <see cref="Mvc.IActionResult"/> with a <see cref="SlackResponse"/> as its content.
    /// </summary>
    public class SlackResponse
    {
        private string _text;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackResponse"/> with a given text to post
        /// to the Slack channel from which the WebHook were received.
        /// </summary>
        public SlackResponse(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            _text = text;
        }

        /// <summary>
        /// Gets or sets the text to send to Slack in response to an incoming WebHook request.
        /// </summary>
        [JsonProperty("text")]
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _text = value;
            }
        }
    }
}
