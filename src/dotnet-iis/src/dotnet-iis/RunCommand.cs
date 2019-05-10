using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.DotNet.IIS
{
    [Command("run", Description = "Runs an ASP.NET Core application in IIS Express.")]
    public class RunCommand
    {
        [Argument(0, "<APPROOT>", Description = "The path to the root of the application. Defaults to the current directory.")]
        public string ApplicationRoot { get; set; }

        public async Task<int> OnExecuteAsync(IConsole console)
        {
            if(string.IsNullOrEmpty(ApplicationRoot))
            {
                ApplicationRoot = Directory.GetCurrentDirectory();
            }
            console.WriteLine($"TODO. Launch: {ApplicationRoot}");
            return 0;
        }
    }
}
