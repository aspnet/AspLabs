using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

public class App
{
    public static void Main(string[] args)
    {
        new App().ReadJson(args);
    }

    private void ReadJson(string[] args)
    {
        string inputPath = args[0];

        if (!Path.IsPathRooted(inputPath))
        {
            Console.WriteLine("The file path you entered does not have a root");
            return;
        }

        OpenApiStreamReader reader = new OpenApiStreamReader();
        var diagnostic = new OpenApiDiagnostic();

        try
        {
            string path = Path.GetFullPath(inputPath);
            Stream stream = File.OpenRead(path);
            OpenApiDocument newDocument = reader.Read(stream, out diagnostic);
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("Check to make sure you entered a correct file path because the file was not found.");
            Console.Error.WriteLine(e.Message);
            return;
        }

        catch(Exception e)
        {
            Console.WriteLine("Check the file path you entered for errors.");
            Console.Error.WriteLine(e.Message);
            return;
        }

        if (diagnostic.Errors.Count == 0)
        {
            Console.WriteLine("Read File Successfully");
        }
        else
        {
            foreach (OpenApiError error in diagnostic.Errors)
            {
                Console.WriteLine($"There was an error reading in the file at {error.Pointer}");
                Console.Error.WriteLine(error.Message);
            }
        }
    }

}
