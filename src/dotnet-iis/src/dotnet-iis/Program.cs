using System;
using System.Diagnostics;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace Microsoft.DotNet.IIS
{
    [Command(Description = "Launches and manages ASP.NET Core applications hosted in IIS.")]
    [Subcommand(typeof(RunCommand))]
    internal class Program
    {
        private static int Main(string[] args)
        {
#if DEBUG
            if (args.Any(a => a == "--debug"))
            {
                args = args.Where(a => a != "--debug").ToArray();
                Console.WriteLine($"Ready for debugger to attach. Process ID: {Process.GetCurrentProcess().Id}.");
                Console.WriteLine("Press ENTER to continue.");
                Console.ReadLine();
            }
#endif

            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (CommandLineException clex)
            {
                var oldFg = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(clex.Message);
                Console.ForegroundColor = oldFg;
                return 1;
            }
            catch (Exception ex)
            {
                var oldFg = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("Unhandled exception:");
                Console.Error.WriteLine(ex.ToString());
                Console.ForegroundColor = oldFg;
                return 1;
            }
        }

        public void OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
        }
    }
}
