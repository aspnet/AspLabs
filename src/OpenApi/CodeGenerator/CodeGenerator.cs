using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace CodeGenerator;

public class App
{
    public static void Main(string[] args)
    {

        if (args.Length != 2)
        {
            Console.Error.WriteLine("Please enter two arguments: an input file path and an output file path.");
            Environment.Exit(1);
        }

        var document = ReadJson(args[0]);
        var paths = document?.Paths;

        if (paths is null || paths.Count == 0)
        {
            Console.Error.WriteLine("No path was found in the schema.");
            Environment.Exit(3);
        }

        var fileProperties = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();  

        foreach (var path in paths)
        {
            var operations = path.Value.Operations;
            if (operations is null || operations.Count == 0)
            {
                Console.Error.WriteLine("No operation was found in path.");
                Environment.Exit(4);
            }
            var pathString = path.Key.ToString();
            fileProperties.Add(pathString, new Dictionary<string, Dictionary<string, string>> { });

            foreach (var operation in operations)
            {
                var method = operation.Key.ToString().ToLower();
                method = GetHttpMethod(method);

                if (method == String.Empty)
                {
                    Console.Error.WriteLine($"Unsupported HTTP method found: '{operation.Key}'");
                    Environment.Exit(4);
                }

                fileProperties[pathString].Add(method, new Dictionary<string, string> { });

                var parameters = operation.Value.Parameters;
                string parametersList = "";

                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    parametersList += GetDataTypeKeyword(parameter.Schema) + " " + parameter.Name;

                    if (i < parameters.Count - 1)
                    {
                        parametersList += ", ";
                    }
                }

                fileProperties[pathString][method].Add("parameters", parametersList);

                var responses = operation.Value.Responses;

                foreach (var response in responses)
                {
                    string returnValue;

                    // for responses that doesn't have "content" property
                    // but a description is always required so we will return that
                    if (response.Value.Content == null || response.Value.Content.Count == 0)
                    {
                        returnValue = $"\"{response.Value.Description}\"";
                        fileProperties[pathString][method].Add(response.Key, returnValue);
                        continue;
                    }
                    var schema = response.Value.Content.First().Value.Schema;

                    if (schema?.Type.ToLower() == "array")
                    {
                        returnValue = GetArrayKeyword(schema);
                    }
                    else
                    {
                        returnValue = GetPrimitiveValue(schema);
                    }

                    fileProperties[pathString][method].Add(response.Key, returnValue);
                }
            }
        }

        var page = new MinimalApiTemplate
        {
            FileProperties = fileProperties
        };
        var pageContent = page.TransformText();
        File.AppendAllText(args[1], pageContent);
    }
    private static string GetHttpMethod(string method) => method switch
    {
        "get" => "MapGet",
        "post" => "MapPost",
        "put" => "MapPut",
        "delete" => "MapDelete",
        _ => String.Empty
    };
    private static string GetDataTypeKeyword(OpenApiSchema? schema) => schema?.Type switch
    {
        "string" => "string",
        "integer" => "int",
        "float" => "float",
        "boolean" => "bool",
        "double" => "double",
        _ => String.Empty
    };
    private static string GetArrayKeyword(OpenApiSchema? schema)
    {
        if (schema == null)
        {
            return String.Empty;
        }
        string returnValue = "[";
        while (schema.Items.Type == "array")
        {
            returnValue += ",";
            schema = schema.Items;
        }
        returnValue = "new " + GetDataTypeKeyword(schema.Items) + returnValue + "] {}";
        return returnValue;
    } 
    private static string GetPrimitiveValue(OpenApiSchema? schema) => schema?.Type switch
    {
        "string" => "\"\"",
        "integer" => "0",
        "boolean" => "false",
        "float" => "0.0f",
        "double" => "0.0d",
        _ => String.Empty,
    };
    private static OpenApiDocument? ReadJson(string args)
    {
        if (!Path.IsPathRooted(args))
        {
            Console.Error.WriteLine("The file path you entered does not have a root");
            return null;
        }

        OpenApiStreamReader reader = new OpenApiStreamReader();
        var diagnostic = new OpenApiDiagnostic();

        try
        {
            string path = Path.GetFullPath(args);
            Stream stream = File.OpenRead(path);
            OpenApiDocument newDocument = reader.Read(stream, out diagnostic);
            return newDocument;
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("Check to make sure you entered a correct file path because the file was not found.");
            Console.Error.WriteLine(e.Message);
            Environment.Exit(2);
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine("Check the file path you entered for errors.");
            Console.Error.WriteLine(e.Message);
            Environment.Exit(2);
            return null;
        }
        finally
        {
            if (diagnostic.Errors.Count == 0)
            {
                Console.WriteLine("Read File Successfully");
            }
            else
            {
                foreach (OpenApiError error in diagnostic.Errors)
                {
                    Console.WriteLine($"There was an error reading in the file: {error.Pointer}");
                    Console.Error.WriteLine(error.Message);
                    Environment.Exit(2);
                }
            }
        }
    }
}
