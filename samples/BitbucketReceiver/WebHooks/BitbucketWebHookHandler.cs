using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json.Linq;

namespace BitbucketReceiver.WebHooks
{
    public class BitbucketWebHookHandler : WebHookHandler
    {
        public BitbucketWebHookHandler()
        {
            this.Receiver = "bitbucket";
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // For more information about BitBucket WebHook payloads, please see 
            // 'https://confluence.atlassian.com/bitbucket/event-payloads-740262817.html#EventPayloads-Push'
            JObject entry = context.GetDataOrDefault<JObject>();

            // Extract the action -- for Bitbucket we have only one.
            string action = context.Actions.First();
            switch (action)
            {
                case "repo:push":
                    // Extract information about the repository
                    var repository = entry["repository"].ToObject<BitbucketRepository>();

                    // Information about the user causing the event
                    var actor = entry["actor"].ToObject<BitbucketUser>();

                    // Information about the specific changes
                    foreach (var change in entry["push"]["changes"])
                    {
                        // The previous commit
                        BitbucketTarget oldTarget = change["old"]["target"].ToObject<BitbucketTarget>();

                        // The new commit
                        BitbucketTarget newTarget = change["new"]["target"].ToObject<BitbucketTarget>();
                    }
                    break;

                default:
                    Trace.WriteLine(entry.ToString());
                    break;
            }

            return Task.FromResult(true);
        }
    }
}