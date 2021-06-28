using System;
using Microsoft.AspNetCore.WebHooks;

namespace Microsoft.AspNetCore.WebHooks
{
    public class IntercomWebHookAttribute : WebHookAttribute
    {
        /// <summary>
        /// Instantiates a new <see cref="IntercomWebHookAttribute"/> instance indicating the associated action is a
        /// Intercom WebHook endpoint.
        /// </summary>
        public IntercomWebHookAttribute()
            : base(IntercomConstants.ReceiverName)
        {
        }

    }
}
