// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// The <see cref="SlackField"/> class is used for expression table fields as part of a
    /// <see cref="SlackAttachment"/>, see <c>https://api.slack.com/docs/attachments</c> for details. Fields are
    /// displayed in a table inside the message attachment.
    /// </summary>
    public class SlackField
    {
        private string _title;
        private string _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackField"/> with the given <paramref name="title"/>
        /// and <paramref name="value"/>.
        /// </summary>
        /// <param name="title">
        /// The field title shown as a bold heading above the value text. It cannot contain markup and will be escaped
        /// by the receiver.
        /// </param>
        /// <param name="value">
        /// The field value which may contain Markdown-style formatting as described in
        /// <c>https://api.slack.com/docs/formatting</c>. The value may be multi-line and must be escaped following
        /// Markdown rules.
        /// </param>
        public SlackField(string title, string value)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            _title = title;
            _value = value;
        }

        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        internal SlackField()
        {
        }

        /// <summary>
        /// Gets or sets the field title shown as a bold heading above the value text. It cannot contain markup and
        /// will be escaped by the receiver.
        /// </summary>
        [JsonProperty("title")]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _title = value;
            }
        }

        /// <summary>
        /// Gets or sets the field value. It may contain Markdown-style formatting as described in
        /// <c>https://api.slack.com/docs/formatting</c>. The value may be multi-line and must be escaped following
        /// Markdown rules.
        /// </summary>
        [JsonProperty("value")]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _value = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the field is short enough to be displayed side-by-side with other
        /// fields.
        /// </summary>
        [JsonProperty("short")]
        public bool Short { get; set; }
    }
}
