using System.Threading.Tasks;
using InstaSharp;
using InstaSharp.Endpoints;
using InstaSharp.Models.Responses;
using Microsoft.AspNet.WebHooks;

namespace InstagramReceiver.WebHooks
{
    public class InstagramWebHookHandler : WebHookHandler
    {
        public InstagramWebHookHandler()
        {
            this.Receiver = InstagramWebHookReceiver.ReceiverName;
        }

        public override async Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // Convert the incoming data to a collection of InstagramNotifications
            var notifications = context.GetDataOrDefault<InstagramNotificationCollection>();

            // Get the config used by InstaSharp client
            InstagramConfig config = Dependencies.InstagramConfig;

            // Access media references in notifications
            foreach (var notification in notifications)
            {
                // If we have an access token then get the media using InstaSharp.
                OAuthResponse auth;
                if (Dependencies.Tokens.TryGetValue(notification.UserId, out auth))
                {
                    var media = new Media(config, auth);
                    MediaResponse mediaResponse = await media.Get(notification.Data.MediaId);
                }
            }
        }
    }
}