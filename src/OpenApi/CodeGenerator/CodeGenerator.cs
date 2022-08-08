using Microsoft.OpenApi.Any;
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
            Environment.Exit(1); // Code 1 is for problems with passed in file paths
        }

        var document = ReadJson(args[0]);
        var paths = document?.Paths;

        if (paths is null || paths.Count == 0)
        {
            Console.Error.WriteLine("No path were found in the schema.");
            Environment.Exit(2); // Code 2 is for problems with paths in schema
        }

        var fileProperties = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();  

        foreach (var path in paths)
        {
            var operations = path.Value.Operations;
            if (operations is null || operations.Count == 0)
            {
                Console.Error.WriteLine("No operation was found in path.");
                Environment.Exit(3); // Code 3 is for problems with operations
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
                    Environment.Exit(3);
                }

                fileProperties[pathString].Add(method, new Dictionary<string, string> { });

                var parameters = operation.Value.Parameters;
                string parametersList = String.Empty;

                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.Schema.Type.ToLower() == "array")
                    {
                        parametersList += GetArrayKeyword(parameter.Schema) + " " + parameter.Name;
                    }
                    else if (parameter.Schema.Type.ToLower() == "object")
                    {
                        parametersList += parameter.Schema.Reference?.Id + $" user{parameter.Schema.Reference?.Id}";
                    }
                    else
                    {
                        parametersList += GetDataTypeKeyword(parameter.Schema) + " " + parameter.Name;
                    }

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

                    // some responses doesn't have "content" property
                    // so these would later return a default value
                    if (response.Value.Content == null || response.Value.Content.Count == 0)
                    {
                        returnValue = "Default";
                        fileProperties[pathString][method].Add(response.Key, returnValue);
                        continue;
                    }
                    var content = response.Value.Content.First().Value;
                    var schema = content.Schema;

                    if (schema?.Type.ToLower() == "array")
                    {
                        returnValue = "new " + GetArrayKeyword(schema) + " {}";
                    }
                    else if (schema?.Type.ToLower() == "object")
                    {
                        returnValue = "new " + schema?.Reference?.Id + "()";
                    }
                    else
                    {
                        returnValue = GetPrimitiveValue(schema);
                    }

                    // this code below is for parsing sample values
                    // this is used for demoing the project
                    if (content.Example != null)
                    {
                        returnValue = GetSampleValue(content.Example, schema);
                    }
                    
                    if (content.Examples != null && content.Examples.Count > 0)
                    {
                        returnValue = GetSampleValue(content.Examples.First().Value.Value, schema);
                    }

                    fileProperties[pathString][method].Add(response.Key, returnValue);
                }
            }
        }

        var schemas = document?.Components?.Schemas;

        Dictionary<string, Dictionary<string, string>> schemaDict = new Dictionary<string, Dictionary<string, string>> ();
        if (schemas != null && schemas.Count > 0)
        {
            foreach (var schema in schemas)
            {
                schemaDict.Add(schema.Key, new Dictionary<string, string>());
                foreach (var property in schema.Value.Properties)
                {
                    string propertyType;
                    if (property.Value.Type.ToLower() == "array")
                    {
                        propertyType = GetArrayKeyword(property.Value);
                    }
                    else if (property.Value.Reference?.Id != null)
                    {
                        propertyType = property.Value.Reference.Id;
                    }
                    else
                    {
                        propertyType = GetDataTypeKeyword(property.Value);
                    }
                    schemaDict[schema.Key].Add(property.Key, propertyType);
                }
            }
        }

        var page = new MinimalApiTemplate
        {
            FileProperties = fileProperties,
            Schemas = schemaDict
        };

        var pageContent = page.TransformText();
        File.WriteAllText(args[1], pageContent);
    }
    private static string GetSampleValue(IOpenApiAny example, OpenApiSchema? schema) => example switch
    {
        OpenApiString castedExample => $"\"{castedExample.Value}\"",
        OpenApiInteger castedExample => castedExample.Value.ToString(),
        OpenApiBoolean castedExample => castedExample.Value.ToString(),
        OpenApiFloat castedExample => castedExample.Value.ToString(),
        OpenApiDouble castedExample => castedExample.Value.ToString(),
        OpenApiArray castedExample => "new " + GetDataTypeKeyword(schema) + $"[] {{{GetArrayValues(castedExample)}}}",
        OpenApiObject castedExample => GetObjectArguments(castedExample, schema),
        OpenApiNull castedExample => "null",
        _ => string.Empty
    };
    private static string GetArrayValues(OpenApiArray example)
    {
        int count = example.Count;
        string returnValue = string.Empty;
        foreach (var value in example)
        {
            returnValue += GetSampleValue(value,null);
            if (count > 1)
            {
                returnValue += ", ";
            }
            count--;
        }
        return returnValue;
    }
    private static string GetObjectArguments(OpenApiObject example, OpenApiSchema? schema)
    {
        string arguments = $"new {schema?.Reference?.Id}(";
        for (int i = 0; i < example.Values.Count; i++)
        {
            if (schema?.Properties?.Values.Count > i)
            {
                arguments += $"{GetSampleValue(example.Values.ElementAt(i), schema?.Properties?.Values?.ElementAt(i))}, ";
            }
            else
            {
                arguments += $"{GetSampleValue(example.Values.ElementAt(i), null)}, ";
            }
        }
        return arguments.Substring(0, arguments.Length - 2) + ")";
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
        if (schema.Items.Type.ToLower() == "object")
        {
            returnValue = schema?.Items.Reference?.Id + returnValue + "]";
        }
        else
        {
            returnValue = GetDataTypeKeyword(schema?.Items) + returnValue + "]";
        }
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
            Environment.Exit(1);
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine("Check the file path you entered for errors.");
            Console.Error.WriteLine(e.Message);
            Environment.Exit(1);
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
                foreach (var error in diagnostic.Errors)
                {
                    Console.WriteLine($"There was an error reading in the file: {error.Pointer}");
                    Console.Error.WriteLine(error.Message);
                    Environment.Exit(1);
                }
            }
      
        }
    }
}
