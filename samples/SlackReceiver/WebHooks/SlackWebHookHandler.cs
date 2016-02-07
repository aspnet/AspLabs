using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace SlackReceiver.WebHooks
{
    public class SlackWebHookHandler : WebHookHandler
    {
        public SlackWebHookHandler()
        {
            this.Receiver = SlackWebHookReceiver.ReceiverName;
        }

        public override Task ExecuteAsync(string generator, WebHookHandlerContext context)
        {
            // For more information about Slack WebHook payloads, please see 
            // 'https://api.slack.com/outgoing-webhooks'
            NameValueCollection command = context.GetDataOrDefault<NameValueCollection>();

            // We can trace to see what is going on.
            Trace.WriteLine(command.ToString());

            // Switch over the IDs we used when configuring this WebHook 
            switch (context.Id)
            {
                case "trigger":
                    // Parse the trigger text of the form 'action parameters'.
                    var triggerCommand = SlackCommand.ParseActionWithValue(command["subtext"]);

                    // Information can be returned using a SlackResponse
                    string reply1 = string.Format(
                        "Received trigger '{0}' with action '{1}' and value '{2}'",
                        command["trigger_word"],
                        triggerCommand.Key,
                        triggerCommand.Value);
                    var triggerReply = new SlackResponse(reply1);
                    context.Response = context.Request.CreateResponse(triggerReply);
                    break;

                case "slash":
                    // Parse the slash text of the form 'action p1=v1; p2=v2; ...'.
                    var slashCommand = SlackCommand.ParseActionWithParameters(command["text"]);

                    string reply2 = string.Format(
                        "Received slash command '{0}' with action '{1}' and value '{2}'",
                        command["command"],
                        slashCommand.Key,
                        slashCommand.Value.ToString());

                    // Information can be returned using a SlackSlashResponse with attachments 
                    var slashReply = new SlackSlashResponse(reply2);

                    // Slash replies can be augmented with attachments containing data, images, and more 
                    var att = new SlackAttachment("Attachment Text", "Fallback description")
                    {
                        Color = "#439FE0",
                        Pretext = "Hello from ASP.NET WebHooks!",
                        Title = "Attachment title",
                    };

                    // Slash attachments can contain tabular data as well
                    att.Fields.Add(new SlackField("Field1", "1234"));
                    att.Fields.Add(new SlackField("Field2", "5678"));

                    // A reply can contain multiple attachments
                    slashReply.Attachments.Add(att);

                    // Return slash command response
                    context.Response = context.Request.CreateResponse(slashReply);
                    break;
            }

            return Task.FromResult(true);
        }
    }
}