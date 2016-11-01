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

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            throw new NotImplementedException();
            //return View();
        }

        public async Task<IActionResult> Contact()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"https://httpbin.org/delay/3");
            }
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
