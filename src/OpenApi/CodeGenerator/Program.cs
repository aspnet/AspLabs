using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

class App
{
    public static void Main(string[] args)
    {
        OpenApiDiagnostic diagnostic = new OpenApiDiagnostic();
        var reader = new OpenApiStreamReader();

        Stream stream = File.OpenRead(args[0]);
        var newDocument = reader.Read(stream, out diagnostic);
        Console.WriteLine("File Read Successful");
    }
}
