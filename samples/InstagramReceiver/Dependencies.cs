using System.Web.Http;
using Microsoft.AspNet.WebHooks;

namespace InstagramReceiver
{
    public static class Dependencies
    {
        private static InstagramWebHookClient _client;

        public static InstagramWebHookClient Client
        {
            get { return _client; }
        }

        public static void Initialize(HttpConfiguration config)
        {
            _client = new InstagramWebHookClient(config);
        }
    }
}