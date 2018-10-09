using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HealthChecks
{

    public class HealthCheckTagHelper : TagHelper
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthCheckTagHelper(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }
        public bool SuppressOnUnhealthy { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // an async tag helper doesn't work here. 
            // it cannot manipulate the output.
            var task = Task.Run(async () =>
                    await _healthCheckService.CheckHealthAsync());
            task.Wait();
            var healthy = task.Result;

            // default suppress on healthy
            var suppressOutput = healthy;

            if (SuppressOnUnhealthy)
            {
                // suppress on unhealthy
                suppressOutput = !healthy;
            }

            if (suppressOutput)
            {
                output.SuppressOutput();
            }
        }
    }
}
