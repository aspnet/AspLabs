using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SampleHealthChecker.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("http://localhost:5050");
            var result = "healthy";

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                result = "unhealthy!";
            }

            ViewData["AppStatus"] = result;

            return View();
        }

    }
}
