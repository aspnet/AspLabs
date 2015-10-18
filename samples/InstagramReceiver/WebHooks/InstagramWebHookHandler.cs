using System;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Newtonsoft.Json.Linq;

namespace InstagramReceiver.WebHooks
{
    public class InstagramWebHookHandler : WebHookHandler
    {
        public InstagramWebHookHandler()
        {
            this.Receiver = "instagram";
        }

        public override async Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // Get the WebHook client
            InstagramWebHookClient client = Dependencies.Client;

            // Convert the incoming data to a collection of InstagramNotifications
            var notifications = context.GetDataOrDefault<InstagramNotificationCollection>();
            foreach (var notification in notifications)
            {
                // Use WebHook client to get detailed information about the posted media
                JArray entries = await client.GetRecentGeoMedia(context.Id, notification.ObjectId);
                foreach (JToken entry in entries)
                {
                    InstagramPost post = entry.ToObject<InstagramPost>();

                    // Image information
                    if (post.Images != null)
                    {
                        InstagramMedia thumbnail = post.Images.Thumbnail;
                        InstagramMedia lowRes = post.Images.LowResolution;
                        InstagramMedia stdRes = post.Images.StandardResolution;
                    }

                    // Video information
                    if (post.Videos != null)
                    {
                        InstagramMedia lowBandwidth = post.Videos.LowBandwidth;
                        InstagramMedia lowRes = post.Videos.LowResolution;
                        InstagramMedia stdRes = post.Videos.StandardResolution;
                    }

                    // Get direct links and sizes of media
                    Uri link = post.Link;
                }
            }
        }
    }
}