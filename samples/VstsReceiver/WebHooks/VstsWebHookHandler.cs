using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Payloads;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace VstsReceiver.WebHooks
{
    /// <summary>
    /// This handler processes WebHooks from Visual Studio Team Services and leverages the <see cref="VstsWebHookHandlerBase"/> base handler.
    /// For details about Visual Studio Team Services WebHooks, see <c>https://www.visualstudio.com/en-us/get-started/integrate/service-hooks/webhooks-and-vso-vs</c>.
    /// </summary>
    public class VstsWebHookHandler : VstsWebHookHandlerBase
    {
        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the <see cref="BuildCompletedPayload"/> WebHook.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, BuildCompletedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the <see cref="TeamRoomMessagePostedPayload"/> WebHook.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, TeamRoomMessagePostedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the <see cref="WorkItemCreatedPayload"/> WebHook.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, WorkItemCreatedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the <see cref="WorkItemCommentedOnPayload"/> WebHook.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, WorkItemCommentedOnPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the <see cref="CodeCheckedInPayload"/> WebHook.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, CodeCheckedInPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the <see cref="WorkItemDeletedPayload"/> WebHook.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, WorkItemDeletedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the <see cref="WorkItemRestoredPayload"/> WebHook.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, WorkItemRestoredPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the <see cref="WorkItemUpdatedPayload"/> WebHook.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, WorkItemUpdatedPayload payload)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// We use <see cref="VstsWebHookHandlerBase"/> so just have to override the methods we want to process WebHooks for.
        /// This one processes the payload for unknown <c>eventType</c>.
        /// </summary>
        public override Task ExecuteAsync(WebHookHandlerContext context, JObject payload)
        {
            return Task.FromResult(true);
        }
    }
}
