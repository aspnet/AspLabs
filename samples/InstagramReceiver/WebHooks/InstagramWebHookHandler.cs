using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace InstagramReceiver.WebHooks
{
    public class InstagramWebHookHandler : WebHookHandler
    {
        private const string MediaEndpointTemplate = "https://api.instagram.com/v1/media/{0}?access_token={1}";

        private static readonly HttpClient _client = new HttpClient();

        public InstagramWebHookHandler()
        {
            this.Receiver = InstagramWebHookReceiver.ReceiverName;
        }

        public override async Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // Get the WebHook client
            InstagramWebHookClient client = Dependencies.Client;

            // Convert the incoming data to a collection of InstagramNotifications
            var notifications = context.GetDataOrDefault<InstagramNotificationCollection>();

            // Access media references in notifications
            foreach (var notification in notifications)
            {
                string token;
                if (Dependencies.Tokens.TryGetValue(notification.UserId, out token))
                {
                    string mediaEndpoint = string.Format(MediaEndpointTemplate, notification.Data.MediaId, token);
                    using (var response = await _client.GetAsync(mediaEndpoint))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            InstagramPost post = await response.Content.ReadAsAsync<InstagramPost>();
                        }
                    }
                }
            }
        }
    }
}