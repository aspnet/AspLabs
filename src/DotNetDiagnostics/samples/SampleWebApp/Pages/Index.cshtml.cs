using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SampleWebApp.Services;

namespace SampleWebApp.Pages
{
    public class IndexModel : PageModel
    {
        public IList<string> Items { get; private set; }

        public void OnGet([FromServices] DataService dataService)
        {
            Items = dataService.GetItems();
        }
    }
}
