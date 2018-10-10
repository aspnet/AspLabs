using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitbucketStronglyTypedCoreReceiver
{
    /// <summary>
    /// Contains information sent about an individual change in a Bitbucket <c>repo:push</c> notification. Ignores
    /// information such as links found at this level in the notification as well as a few <see cref="bool"/>
    /// properties.
    /// </summary>
    public class BitbucketChange
    {
        [JsonProperty("old")]
        public BitbucketState Old { get; set; }

        [JsonProperty("new")]
        public BitbucketState New { get; set; }

        [JsonProperty("commits", Required = Required.Always)]
        public IList<BitbucketCommit> Commits { get; } = new List<BitbucketCommit>();

        [JsonProperty("truncated", Required = Required.Always)]
        public bool AreCommitsTruncated { get; set; }
    }
}
