using Microsoft.AspNetCore.WebHooks;
using Newtonsoft.Json;

namespace BitbucketStronglyTypedCoreReceiver
{
    /// <summary>
    /// Contains information sent in a Bitbucket WebHook <c>repo:push</c> notification.
    /// </summary>
    public class BitbucketPushNotification
    {
        [JsonProperty("actor", Required = Required.Always)]
        public BitbucketUser Actor { get; set; }

        [JsonProperty("repository", Required = Required.Always)]
        public BitbucketRepository Repository { get; set; }

        [JsonProperty("push", Required = Required.Always)]
        public BitbucketPush Push { get; set; }
    }
}
