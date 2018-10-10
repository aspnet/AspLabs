using Microsoft.AspNetCore.WebHooks;
using Newtonsoft.Json;

namespace BitbucketStronglyTypedCoreReceiver
{
    /// <summary>
    /// Contains information sent about an individual commit in a Bitbucket <c>repo:push</c> notification. Ignores
    /// information such as links found at this level in the notification.
    /// </summary>
    public class BitbucketCommit
    {
        [JsonProperty("author", Required = Required.Always)]
        public BitbucketUser Author { get; set; }

        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; set; }

        [JsonProperty("hash", Required = Required.Always)]
        public string Sha1Hash { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }
    }
}
