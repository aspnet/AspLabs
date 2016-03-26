using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Http;
using InstaSharp;
using InstaSharp.Models.Responses;

namespace InstagramReceiver
{
    public static class Dependencies
    {
        private static InstagramConfig _config;
        private static ConcurrentDictionary<string, OAuthResponse> _tokens;

        /// <summary>
        /// Gets the <see cref="InstagramConfig"/> used by Instasharp, see <c>http://instasharp.org/</c>.
        /// </summary>
        public static InstagramConfig InstagramConfig
        {
            get { return _config; }
        }

        /// <summary>
        /// Gets cached Instagram access tokens from logged in users.
        /// </summary>
        public static IDictionary<string, OAuthResponse> Tokens
        {
            get { return _tokens; }
        }

        public static void Initialize(HttpConfiguration config)
        {
            var clientId = WebConfigurationManager.AppSettings["MS_WebHookReceiverSecret_InstagramId"];
            var clientSecret = WebConfigurationManager.AppSettings["MS_WebHookReceiverSecret_Instagram"];
            var webHookHost = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME") ?? "localhost";

            // Note: you can use the 'id' field of the callbackURI to manage multiple subscriptions with each their callback.
            var callbackUri = string.Format("https://{0}/api/webhooks/incoming/instagram", webHookHost);
            _config = new InstagramConfig(clientId, clientSecret, redirectUri: null, callbackUri: callbackUri);

            _tokens = new ConcurrentDictionary<string, OAuthResponse>();
        }
    }
}