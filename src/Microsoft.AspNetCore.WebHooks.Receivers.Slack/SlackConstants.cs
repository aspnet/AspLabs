// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.WebHooks
{
    /// <summary>
    /// Well-known names used in Slack receivers and handlers.
    /// </summary>
    public static class SlackConstants
    {
        /// <summary>
        /// Gets the name of the posted value in a Slack WebHook request body containing the channel name.
        /// </summary>
        public static string ChannelRequestFieldName => "channel_name";

        /// <summary>
        /// Gets the name of the posted value in a Slack WebHook request body containing the command name. This value
        /// is somewhat analogous to an event name but it is optional in a request.
        /// </summary>
        public static string CommandRequestFieldName => "command";

        /// <summary>
        /// Gets the name of the Slack WebHook receiver.
        /// </summary>
        public static string ReceiverName => "slack";

        /// <summary>
        /// Gets the minimum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMinLength => 16;

        /// <summary>
        /// Gets the maximum length of the secret key configured for this receiver.
        /// </summary>
        public static int SecretKeyMaxLength => 128;

        /// <summary>
        /// Gets the name of the <see cref="AspNetCore.Routing.RouteValueDictionary"/> entry containing the subtext
        /// value for the current request. This entry contains the portion of the <see cref="TextRequestFieldName"/>
        /// value which does not match the <see cref="TriggerRequestFieldName"/> value if
        /// <see cref="TriggerRequestFieldName"/> is non-<see langword="null"/>. This entry contains the
        /// <see cref="TextRequestFieldName"/> value if both <see cref="CommandRequestFieldName"/> and
        /// <see cref="TriggerRequestFieldName"/> values are <see langword="null"/>. Otherwise, the
        /// <see cref="AspNetCore.Routing.RouteValueDictionary"/> will not contain this entry.
        /// </summary>
        public static string SubtextRequestKeyName => "subtext";

        /// <summary>
        /// Gets the name of a parameter bound to the Slack subtext value for the current request.
        /// </summary>
        /// <seealso cref="SubtextRequestKeyName"/>
        public static string SubtextParameterName => "subtext";

        /// <summary>
        /// Gets the name of the posted value in a Slack WebHook request body containing the text of the event. This is
        /// used as a fallback event name when neither <see cref="CommandRequestFieldName"/> nor
        /// <see cref="TriggerRequestFieldName"/> is included in a request.
        /// </summary>
        public static string TextRequestFieldName => "text";

        /// <summary>
        /// Gets the name of the posted value in a Slack WebHook request body containing the shared-private security
        /// token.
        /// </summary>
        public static string TokenRequestFieldName => "token";

        /// <summary>
        /// Gets the name of the posted value in a Slack WebHook request body containing the trigger word which caused
        /// this event, if any. This value is somewhat analogous to an event name and it is used when
        /// <see cref="CommandRequestFieldName"/> is not included in a request. It too is optional.
        /// </summary>
        public static string TriggerRequestFieldName => "trigger_word";
    }
}
