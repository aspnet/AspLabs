using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Http;
using Microsoft.AspNet.WebHooks;

namespace InstagramReceiver
{
    public static class Dependencies
    {
        private static InstagramWebHookClient _client;
        private static ConcurrentDictionary<string, string> _tokens;

        /// <summary>
        /// Gets the <see cref="InstagramWebHookClient"/> used to subscribe to Instagram for WebHook notifications.
        /// </summary>
        public static InstagramWebHookClient Client
        {
            get { return _client; }
        }

        /// <summary>
        /// Gets cached Instagram access tokens from logged in users.
        /// </summary>
        public static IDictionary<string, string> Tokens
        {
            get { return _tokens; }
        }

        public static void Initialize(HttpConfiguration config)
        {
            _client = new InstagramWebHookClient(config);
            _tokens = new ConcurrentDictionary<string, string>();
        }
    }
}