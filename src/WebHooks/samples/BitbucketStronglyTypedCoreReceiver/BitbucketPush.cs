using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BitbucketStronglyTypedCoreReceiver
{
    /// <summary>
    /// Contains information sent about the push in a Bitbucket WebHook <c>repo:push</c> notification.
    /// </summary>
    public class BitbucketPush
    {
        [JsonProperty("changes", Required = Required.Always)]
        [MinLength(1)]
        public BitbucketChange[] Changes { get; set; } = Array.Empty<BitbucketChange>();
    }
}
