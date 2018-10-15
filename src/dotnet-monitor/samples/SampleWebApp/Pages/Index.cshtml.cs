using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace SampleWebApp.Pages
{
    public class IndexModel : PageModel
    {
        public IEnumerable<int> SelfInvokedValues { get; set; }

        public async Task OnGetAsync()
        {
            // This is a no-no but it's nice for illustration :).
            var client = new HttpClient();
            var resp = await client.GetAsync($"https://{HttpContext.Request.Host}/api/values");
            resp.EnsureSuccessStatusCode();
            SelfInvokedValues = JsonConvert.DeserializeObject<List<int>>(await resp.Content.ReadAsStringAsync());
        }
    }
}
