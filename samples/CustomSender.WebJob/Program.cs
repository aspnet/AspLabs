using System.Configuration;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.Azure.WebJobs;

namespace CustomSender.WebJob
{
    internal class Program
    {
        /// <summary>
        /// Gets or sets the <see cref="IWebHookManager"/> instance to use.
        /// </summary>
        public static IWebHookManager Manager { get; set; }

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
            Manager = new WebHookManager(store, sender, logger);

            // Initialize WebJob
            var listener = ConfigurationManager.ConnectionStrings["WebHookListener"].ConnectionString;
            JobHostConfiguration config = new JobHostConfiguration
            {
                StorageConnectionString = listener
            };
            JobHost host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
