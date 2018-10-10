using Microsoft.AspNetCore.WebHooks;
using Newtonsoft.Json;

namespace BitbucketStronglyTypedCoreReceiver
{
    /// <summary>
    /// Contains information sent about the <c>old</c> or <c>new</c> state of an individual change in a Bitbucket
    /// <c>repo:push</c> notification. Ignores information such as links found at this level in the notification.
    /// </summary>
    public class BitbucketState
    {
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("target", Required = Required.Always)]
        public BitbucketTarget Target { get; set; }
    }
}
