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
            var result = await _healthCheck.CheckHealthAsync();

            ViewData["Results"] = JsonConvert.SerializeObject(result); 
            ViewData["AppStatus"] = result;

            return View();
        }

    }
}
