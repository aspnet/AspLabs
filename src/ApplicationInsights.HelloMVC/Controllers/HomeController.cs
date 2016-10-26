using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HelloMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        //[HttpGet("{timeout}")]
        public async Task<string> Wait(int timeout)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://httpbin.org/delay/{timeout}");
                return response.StatusCode.ToString();
            }

        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            throw new NotImplementedException();
            //return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
