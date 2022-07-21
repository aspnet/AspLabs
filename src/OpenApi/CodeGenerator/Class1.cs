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

            foreach (var path in paths)
            {
                var operations = path.Value.Operations;
                foreach (var operation in operations)
                {
                    var method = operation.Key.ToString().ToLower();

                    switch(method)
                    {
                        case "get":
                            page = new RuntimeTextTemplate2
                            {
                                apiPath = path.Key.ToString(),
                                apiMethod = "MapGet"
                            };
                            pageContent = page.TransformText();
                            File.AppendAllText("C:\\Users\\AnhThiDao\\AspLabs\\src\\OpenAPI\\OutputFile\\Program.cs", pageContent);
                            break;

                        case "post":
                            page = new RuntimeTextTemplate2
                            {
                                apiPath = path.Key.ToString(),
                                apiMethod = "MapPost"
                            };
                            pageContent = page.TransformText();
                            File.AppendAllText("C:\\Users\\AnhThiDao\\AspLabs\\src\\OpenAPI\\OutputFile\\Program.cs", pageContent);
                            break;

                        case "put":
                            page = new RuntimeTextTemplate2
                            {
                                apiPath = path.Key.ToString(),
                                apiMethod = "MapPut"
                            };
                            pageContent = page.TransformText();
                            File.AppendAllText("C:\\Users\\AnhThiDao\\AspLabs\\src\\OpenAPI\\OutputFile\\Program.cs", pageContent);
                            break;

                        case "delete":
                            page = new RuntimeTextTemplate2
                            {
                                apiPath = path.Key.ToString(),
                                apiMethod = "MapDelete"
                            };
                            pageContent = page.TransformText();
                            File.AppendAllText("C:\\Users\\AnhThiDao\\AspLabs\\src\\OpenAPI\\OutputFile\\Program.cs", pageContent);
                            break;
                    }
                }
            }

            //RuntimeTextTemplate2 page = new RuntimeTextTemplate2
            //{
            //    path = "\"/students\""
            //};
            //String pageContent = page.TransformText();
            //System.IO.File.WriteAllText(args[1], pageContent);
        }

        private OpenApiDocument ReadJson(string args)
        {
            string inputPath = "C:\\Users\\AnhThiDao\\openapi.json";



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
