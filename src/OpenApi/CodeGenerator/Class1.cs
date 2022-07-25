namespace CodeGenerator
{
    using Microsoft.OpenApi.Models;
    using Microsoft.OpenApi.Readers;
    public class App
    {
        public static void Main(string[] args)
        {
            var document = new App().ReadJson("");
            var paths = document.Paths;
            //var operation = paths.Values.FirstOrDefault().Operations[0];
            //var responseDescription = operation.Responses["200"];

            RuntimeTextTemplate2 page;
            String pageContent;
            int countPaths = 0;
            bool shouldCreateWebApp = true;

            foreach (var path in paths)
            {
                var operations = path.Value.Operations;
                foreach (var operation in operations)
                {
                    if (countPaths > 0)
                    {
                        shouldCreateWebApp = false;
                    }

                    var method = operation.Key.ToString().ToLower();
                    var response = operation.Value.Responses.FirstOrDefault().Value;
                    var schema = response.Content.Values.FirstOrDefault()?.Schema;

                    var parameters = operation.Value.Parameters;
                    string parametersList = "";
                    int countParam = 1;

                    foreach (var parameter in parameters)
                    {
                        parametersList += new App().GetDataTypeKeyword(parameter.Schema) + " " + parameter.Name;

                        if (countParam < parameters.Count)
                        {
                            parametersList += ", ";
                        }

                        countParam++;
                    }

                    string returnValue;
                    if (schema?.Type.ToLower() == "array")
                    {
                        returnValue = new App().GetDataTypeKeyword(schema);
                        returnValue = "new " + returnValue + " {}";
                    }
                    else
                    {
                        returnValue = new App().GetPrimitiveValue(schema);
                    }

                    page = new RuntimeTextTemplate2
                    {
                        path = path.Key.ToString(),
                        method = new App().GetHttpMethod(method),
                        shouldCreateWebApp = shouldCreateWebApp,
                        returnValue = returnValue,
                        parametersList = parametersList
                    };
                    pageContent = page.TransformText();
                    File.AppendAllText("C:\\Users\\AnhThiDao\\AspLabs\\src\\OpenApi\\OutputFile\\Program.cs", pageContent);

                    countPaths++;
                }
            }

            //    //RuntimeTextTemplate2 page = new RuntimeTextTemplate2
            //    //{
            //    //    path = "\"/students\""
            //    //};
            //    //String pageContent = page.TransformText();
            //    //System.IO.File.WriteAllText(args[1], pageContent);

        }
        private string GetHttpMethod(string method)
        {
            switch (method)
            {
                case "get":
                    return "MapGet";

                case "post":
                    return "MapPost";

                case "put":
                    return "MapPut";

                case "delete":
                    return "MapDelete";

                default:
                    return "";
            }
        }
        private string GetDataTypeKeyword(OpenApiSchema? schema)
        {
            var type = schema?.Type;

            switch (type)
            {
                case "string":
                    return "string";

                case "integer":
                    return "int";

                case "boolean":
                    return "bool";

                case "float":
                    return "float";

                case "double":
                    return "double";

                case "array":
                    return new App().GetDataTypeKeyword(schema?.Items) + "[]";
            }
            return "";
        }
        private string GetPrimitiveValue(OpenApiSchema? schema)
        {
            var type = schema?.Type;

            switch (type)
            {
                case "string":
                    return "\"\"";

                case "integer":
                    return "0";

                case "boolean":
                    return "false";

                case "float":
                    return "0.0f";

                case "double":
                    return "0.0d";
            }
            return "";
        }
        private OpenApiDocument ReadJson(string args)
        {
            //var inputPath = "C:\\Users\\AnhThiDao\\openapi.json";
            var inputPath = "C:\\Users\\Anh Thi Dao\\Downloads\\petstore.json";

            if (!Path.IsPathRooted(inputPath))
            {
                Console.WriteLine("The file path you entered does not have a root");
                return new OpenApiDocument();
            }

            OpenApiStreamReader reader = new OpenApiStreamReader();
            var diagnostic = new OpenApiDiagnostic();

            try
            {
                string path = Path.GetFullPath(inputPath);
                Stream stream = File.OpenRead(path);
                OpenApiDocument newDocument = reader.Read(stream, out diagnostic);
                return newDocument;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Check to make sure you entered a correct file path because the file was not found.");
                Console.Error.WriteLine(e.Message);
                return new OpenApiDocument();
            }
            catch (Exception e)
            {
                Console.WriteLine("Check the file path you entered for errors.");
                Console.Error.WriteLine(e.Message);
                return new OpenApiDocument();
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
                        Console.WriteLine($"There was an error reading in the file at {error.Pointer}");
                        Console.Error.WriteLine(error.Message);
                    }
                }
            }
        }



        // public OpenApiOperation CreateOperation(OpenApiDocument document)
        // {
        //     OpenApiOperation operation = new OpenApiOperation();
        //     OpenApiPathItem key = new OpenApiPathItem()
        //     operation = document.Paths.Values;
        // }
    }

}
