using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;
using Microsoft.AspNet.WebHooks.Diagnostics;
using Microsoft.AspNet.WebHooks.Services;

namespace CustomSender.QueuedSender
{
    internal class Program
    {
        private const string QueueConnectionString = "MS_AzureStoreConnectionString";

        public static void Main(string[] args)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => DequeueAndSendWebHooks(cancellationTokenSource.Token));

            Console.WriteLine("Hit ENTER to exit!");
            Console.ReadLine();

            cancellationTokenSource.Cancel();
        }

        private static async Task DequeueAndSendWebHooks(CancellationToken cancellationToken)
        {
            // Create the dequeue manager
            string connectionString = ConfigurationManager.ConnectionStrings[QueueConnectionString].ConnectionString;
            ILogger logger = CommonServices.GetLogger();
            AzureWebHookDequeueManager manager = new AzureWebHookDequeueManager(connectionString, logger);

            // Start the dequeue manager
            await manager.Start(cancellationToken);
        }
    }
}
