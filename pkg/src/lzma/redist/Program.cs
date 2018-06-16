using System;
using System.IO;
using Microsoft.DotNet.Archive;

class Program
{
    public static void Main(string[] args)
    {
        var source = Path.GetFullPath(args[0]).TrimEnd(new []{ '\\', '/' });
        var outputPath = Path.GetFullPath(args[1]);

        var progress = new ConsoleProgressReport();
        using (var archive = new IndexedArchive())
        {
            Console.WriteLine($"Adding directory: {source}");
            archive.AddDirectory(source, progress);
            archive.Save(outputPath, progress);
        }
    }
}
