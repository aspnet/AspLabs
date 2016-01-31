using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Microsoft.Azure.WebJobs;

namespace CustomSender.WebJob
{
    public class Functions
    {
        /// <summary>
        /// This method is triggered when a message arrives on the 'listener' queue on the
        /// the 'WebHookListener' Azure Storage Account. 
        /// </summary>
        public static async Task ProcessQueueMessageAsync([QueueTrigger("listener")] string message, TextWriter logger)
        {
            await logger.WriteLineAsync(message);

            // Send message to all subscribers as WebHooks. Use a predicate to filter
            // which receivers should get a WebHook request.
            await Program.Manager.NotifyAllAsync("event1", new { Message = message });
        }
    }
}
