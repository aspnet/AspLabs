// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// The <see cref="SlackAttachment"/> is used to describe the contents of an <see cref="SlackSlashResponse"/>.
    /// </summary>
    public class SlackAttachment
    {
        private readonly Collection<SlackField> _fields = new Collection<SlackField>();

        private string _text;
        private string _fallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackAttachment"/> class with the given
        /// <paramref name="text"/> and <paramref name="fallback"/>.
        /// </summary>
        /// <param name="text">The main text in a message attachment. The text may contain Markdown-style formatting
        /// as described in <c>https://api.slack.com/docs/formatting</c>. The contents will automatically be collapsed
        /// if it contains more than 700 characters or more than 5 line breaks. In this case it will be displayed with
        /// a <c>"Show more..."</c> link to the contents.
        /// </param>
        /// <param name="fallback">A plain-text summary of the attachment which will be used in clients
        /// that don't show formatted text (e.g. IRC, mobile notifications). It should not contain any markup.</param>
        public SlackAttachment(string text, string fallback)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
        }

        /// <summary>
        /// Default constructor for serialization purposes
        /// </summary>
        internal SlackAttachment()
        {
        }

        /// <summary>
        /// Gets or sets a required plain-text summary of the attachment. This text will be used in clients
        /// that don't show formatted text (e.g. IRC, mobile notifications) and should not contain
        /// any markup.
        /// </summary>
        [JsonProperty("fallback")]
        public string Fallback
        {
            get
            {
                return _fallback;
            }
            set
            {
                _fallback = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets an optional value that can either be one of <c>good</c>, <c>warning</c>, <c>danger</c>,
        /// or any hex color code (e.g. <c>#439FE0</c>). This value is used to color the border along the left side
        /// of the message attachment.
        /// </summary>
        [JsonProperty("color")]
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets an optional text that appears above the message attachment block.
        /// </summary>
        [JsonProperty("pretext")]
        public string Pretext { get; set; }

        /// <summary>
        /// Gets or sets an optional small text used to display the author's name.
        /// </summary>
        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets a URI that will show up as a hyper link for the <see cref="AuthorName"/> text. This will only
        /// be applied if <see cref="AuthorName"/> is present.
        /// </summary>
        [JsonProperty("author_link")]
        public Uri AuthorLink { get; set; }

        /// <summary>
        /// Gets or sets a URI that display a small 16x16 pixel image to the left of the <see cref="AuthorName"/> text.
        /// This will only be applied if <see cref="AuthorName"/> is present.
        /// </summary>
        [JsonProperty("author_icon")]
        public Uri AuthorIcon { get; set; }

        /// <summary>
        /// Gets or sets an optional title which is displayed as larger, bold text near the top of a message
        /// attachment.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a hyper link for the <see cref="Title"/> text. This will only be applied if
        /// <see cref="Title"/> is present.
        /// </summary>
        [JsonProperty("title_link")]
        public Uri TitleLink { get; set; }

        /// <summary>
        /// Gets or sets the main text in a message attachment. The text may contain Markdown-style formatting as
        /// described in <c>https://api.slack.com/docs/formatting</c>. The contents will automatically be collapsed if
        /// it contains more than 700 characters or more than 5 line breaks. In this case it will be displayed with a
        /// <c>"Show more..."</c> link to the contents.
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
                _text = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or a URI to an image that will be displayed inside a message attachment. Currently supported formats
        /// include GIF, JPEG, PNG, and BMP.
        /// </summary>
        [JsonProperty("image_url")]
        public Uri ImageLink { get; set; }

        /// <summary>
        /// Gets or a URI to an image that will be displayed as a thumbnail on the right side of a message attachment.
        /// Currently supported formats include GIF, JPEG, PNG, and BMP.
        /// </summary>
        [JsonProperty("thumb_url")]
        public Uri ThumbLink { get; set; }

        /// <summary>
        /// Gets a set of <see cref="SlackField"/> instances that will be displayed in a table inside the message
        /// attachment
        /// </summary>
        [JsonProperty("fields")]
        public Collection<SlackField> Fields
        {
            get
            {
                return _fields;
            }
        }
    }
}
