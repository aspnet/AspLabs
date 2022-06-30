using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

class App
{
    //Things to read in from the JSON: 
    //Info (which includes the version and the Title)
    //Servers
    //Path
    //For each path we need to also read in the operations
    //For each operation we need to read in the Descriptions and Responses.

    public static void Main(string[] args)
    {
        OpenApiDiagnostic diagnostic = new OpenApiDiagnostic();
        var reader = new OpenApiStreamReader();

        //var path = "C:\\Users\\t-barthur\\Downloads\\openapi.json";

        Stream stream = File.OpenRead(args[0]);
        var newDocument = reader.Read(stream, out diagnostic);
        Console.WriteLine("File Read Successful");
    }
}
