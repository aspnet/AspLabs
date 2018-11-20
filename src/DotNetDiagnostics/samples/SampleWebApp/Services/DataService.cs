using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace SampleWebApp.Services
{
    public class DataService
    {
        private readonly ILogger<DataService> _logger;
        private List<byte[]> _data = new List<byte[]>();

        public DataService(ILogger<DataService> logger)
        {
            logger.LogInformation("Data service initialized");
            _logger = logger;

            // Simulate allocating a bunch of memory.
            for (var i = 0; i < 1000; i++)
            {
                _data.Add(new byte[4096]);
            }
        }

        public IList<string> GetItems()
        {
            _logger.LogInformation("Getting items");
            return new[] { "a", "b", "c", "d" };
        }
    }
}