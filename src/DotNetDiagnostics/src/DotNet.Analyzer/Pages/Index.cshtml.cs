using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNet.Analyzer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Diagnostics.Runtime;

namespace DotNet.Analyzer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DumpService _dumpService;

        public DataTarget ActiveDump { get; set; }

        public IndexModel(DumpService dumpService)
        {
            _dumpService = dumpService;
        }

        public void OnGet()
        {
            ActiveDump = _dumpService.Dump;
        }

        public async Task OnPost(string path)
        {
            await _dumpService.LoadDumpAsync(path);
            OnGet();
        }
    }
}
