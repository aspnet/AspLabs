gRPC HTTP API
=============

gRPC HTTP API is an experimental extension for ASP.NET Core that creates RESTful HTTP JSON APIs for gRPC services. Once configured, gRPC HTTP API allows apps to call gRPC services with familiar HTTP concepts:

* HTTP verbs
* URL parameter binding
* JSON requests/responses

Of course gRPC can continue to be used as well. RESTful APIs for your gRPC services. No duplication!

![gRPC loves REST](grpc-rest-logo.png "gRPC loves REST")

### Usage

1. Add a package reference to [Microsoft.AspNetCore.Grpc.HttpApi](https://www.nuget.org/packages/Microsoft.AspNetCore.Grpc.HttpApi).
2. Register services in `Startup.cs` with `AddGrpcHttpApi`.
3. Add [google/api/http.proto](https://github.com/aspnet/AspLabs/blob/c1e59cacf7b9606650d6ec38e54fa3a82377f360/src/GrpcHttpApi/sample/Proto/google/api/http.proto) and [google/api/annotations.proto](https://github.com/aspnet/AspLabs/blob/c1e59cacf7b9606650d6ec38e54fa3a82377f360/src/GrpcHttpApi/sample/Proto/google/api/annotations.proto) files to your project.
4. Annotate gRPC methods in your `.proto` files with HTTP bindings and routes:

```protobuf
syntax = "proto3";

import "google/api/annotations.proto";

package greet;

service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply) {
    option (google.api.http) = {
      get: "/v1/greeter/{name}"
    };
  }
}

message HelloRequest {
  string name = 1;
}

message HelloReply {
  string message = 1;
}
```

The `SayHello` gRPC method can now be invoked as gRPC+Protobuf and as an HTTP API:

* Request: `HTTP/1.1 GET /v1/greeter/world`
* Response: `{ "message": "Hello world" }`

Server logs show that the HTTP call is executed by a gRPC service. gRPC HTTP API maps the incoming HTTP request to a gRPC message, and then converts the response message to JSON.

```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET https://localhost:5001/v1/greeter/world
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'gRPC - /v1/greeter/{name}'
info: Server.GreeterService[0]
      Sending hello to world
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'gRPC - /v1/greeter/{name}'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished in 1.996ms 200 application/json
```

This is a basic example. See [HttpRule](https://cloud.google.com/service-infrastructure/docs/service-management/reference/rpc/google.api#google.api.HttpRule) for more customization options.

[`Microsoft.AspNetCore.Grpc.HttpApi`](https://www.nuget.org/packages/Microsoft.AspNetCore.Grpc.HttpApi) is available on NuGet.org.

### gRPC Gateway

[grpc-gateway](https://github.com/grpc-ecosystem/grpc-gateway) maps RESTful HTTP APIs to gRPC using a proxy server. This project adds the same features as grpc-gateway but **without** a proxy.

### Enable Swagger/OpenAPI support

Swagger (OpenAPI) is a language-agnostic specification for describing REST APIs. gRPC HTTP API can integrate with [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) to generate a Swagger endpoint for RESTful gRPC services. The Swagger endpoint can then be used with [Swagger UI](https://swagger.io/swagger-ui/) and other tooling.

To enable Swagger with gRPC HTTP API:

1. Add a package reference to [Microsoft.AspNetCore.Grpc.Swagger](https://www.nuget.org/packages/Microsoft.AspNetCore.Grpc.Swagger).
2. Configure Swashbuckle in `Startup.cs`. The `AddGrpcSwagger` method configures Swashbuckle to include gRPC HTTP API endpoints.

[!code-csharp[](~/grpc/httpapi/Startup.cs?name=snippet_1&highlight=6-10,15-19)]

To confirm that Swashbuckle is generating Swagger for the RESTful gRPC services, start the app and navigate to the Swagger UI page.

### Known issues

##### Protobuf JSON serialization uses the JSON support in `Google.Protobuf`

Issues with this JSON implementation:

* It's blocking (i.e. not async), which requires the input and output to be cached in memory so as not to block ASP.NET Core.
* It's not optimized for performance.
    
Improvement would be to write a new runtime serializer for protobuf types with the same behavior. It would be async, use `System.Text.Json` and cache necessary reflection. An alternative approach would be to write a `protoc` plugin that generates the JSON serialization code.

##### `google/api/annotations.proto` and `google/api/http.proto` need to be added to source code

`google/api/annotations.proto` and `google/api/http.proto` need to be added in the end-user's source code so the Protobuf compiler can load them along with the user's proto files. It would be a nicer developer experience if the user somehow didn't need to worry about those files.

### Experimental project

This project is experimental. It has known issues, it is not complete and it is not supported. We are planning to add a supported implementation of this feature in .NET 7.

We want to gauge developer interest in gRPC HTTP API. If gRPC HTTP API is interesting to you then please [give feedback](https://github.com/grpc/grpc-dotnet/issues/167).
