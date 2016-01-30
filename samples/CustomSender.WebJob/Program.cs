using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.Azure.WebJobs;

namespace CustomSender.WebJob
{
    public class Program
    {
        private const string QueueAddr = "MS_AzureStoreConnectionString";

        private static IWebHookManager _manager;

        public static void Main(string[] args)
        {
            // Set up default WebHook logger
            ILogger logger = new TraceLogger();

            // Set the WebHook Store we want to get WebHook subscriptions from. Azure store requires
            // a valid Azure Storage connection string named MS_AzureStoreConnectionString.
            IWebHookStore store = AzureWebHookStore.CreateStore(logger);

            // Set the sender we want to actually send out the WebHooks. We could also 
            // enqueue messages for scale out.
            IWebHookSender sender = new DataflowWebHookSender(logger);

            // Set up WebHook manager which we use for creating notifications.
            _manager = new WebHookManager(store, sender, logger);

            // Initialize WebJob
            var queueAddr = ConfigurationManager.ConnectionStrings[QueueAddr].ConnectionString;
            JobHostConfiguration config = new JobHostConfiguration
            {
                StorageConnectionString = queueAddr
            };
            JobHost host = new JobHost(config);
            host.RunAndBlock();
        }

        public static async Task ProcessQueueMessageAsync([QueueTrigger("listener")] string message, TextWriter logger)
        {
            await logger.WriteLineAsync(message);

            // Send message to all subscribers as WebHooks
            await _manager.NotifyAllAsync("event1", new { Message = message });
        }
    }
}
