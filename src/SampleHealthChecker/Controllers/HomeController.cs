using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SampleHealthChecker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHealthCheckService _healthCheck;

        public HomeController(IHealthCheckService healthCheck)
        {
            _healthCheck = healthCheck;
        }

        public async Task<IActionResult> Index()
        {
            string result;
            if (await _healthCheck.CheckHealthAsync())
            {
                result = "healthy";
            }
            else
            {
                result = "unhealthy!";
            }

            ViewData["Results"] = JsonConvert.SerializeObject(_healthCheck.CheckResults); 
            ViewData["AppStatus"] = result;

            return View();
        }

    }
}
