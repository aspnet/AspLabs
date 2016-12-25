using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace GenericReceivers.Dependencies
{
    /// <summary>
    /// This provides a sample implementation of <see cref="IWebHookReceiverConfig"/> which gets the 
    /// receiver config from an in-memory structure.
    /// </summary>
    public class ReceiverConfig : IWebHookReceiverConfig
    {
        private static readonly IDictionary<string, IDictionary<string, string>> _secrets =
            new Dictionary<string, IDictionary<string, string>>()
            {
                {
                    GenericJsonWebHookReceiver.ReceiverName,
                    new Dictionary<string, string>
                    {
                        { "", "83699ec7c1d794c0c780e49a5c72972590571fd8" },
                        { "1", "41345df5a2d794c0c350b49a5c42572392511fa1" },
                    }
                }
            };

        public Task<string> GetReceiverConfigAsync(string name, string id)
        {
            IDictionary<string, string> secrets;
            if (_secrets.TryGetValue(name, out secrets))
            {
                string secret;
                if (secrets.TryGetValue(id, out secret))
                {
                    return Task.FromResult(secret);
                }
            }

            return Task.FromResult<string>(null);
        }
    }
}