ASP.NET Core gRPC HTTP API
==========================

This project is an extension for ASP.NET Core gRPC server that creates RESTful HTTP APIs for gRPC services. Once configured, gRPC HTTP API allows you call gRPC methods with familiar HTTP concepts:

* HTTP verbs
* URL parameter binding
* JSON requests/responses

HTTP+JSON APIs for your gRPC services. No duplication!

![gRPC loves REST](grpc-rest-logo.png "gRPC loves REST")

### Usage:

1. Add a package reference to `Microsoft.AspNetCore.Grpc.HttpApi`.
2. Register services in *Startup.cs* with `AddGrpcHttpApi()`.
2. Include `<Protobuf>` reference to *google/api/http.proto* and *google/api/http.proto*.
3. Annotate gRPC methods in your *.proto* file with your HTTP bindings and routes:

```protobuf
syntax = "proto3";

import "google/api/annotations.proto";

package greet;

service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply) {
    option (google.api.http) = {
      get: "v1/greeter/{name}"
    };
  }
}

message HelloRequest {
  string name = 1;
  string test_name = 2;
}

message HelloReply {
  string message = 1;
}
```

The `SayHello` gRPC method can now be invoked as gRPC+Protobuf and as an HTTP API:

* Request: `HTTP/1.1 GET /v1/greeter/world`
* Response: `{ "message": "Hello world" }`

Server logs:

```
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET https://localhost:5001/v1/greeter/world
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'gRPC - v1/greeter/{name}'
info: Server.GreeterService[0]
      Sending hello to world
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'gRPC - v1/greeter/{name}'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished in 1.996ms 200 application/json
```

This is a simple example. See [HttpRule](https://cloud.google.com/service-infrastructure/docs/service-management/reference/rpc/google.api#google.api.HttpRule) for more customization options.

### Known issues:

1. `Google.Protobuf` has a bug in the latest version that prevents it from loading annotations. Bug is worked around by using version 3.8.0. GitHub issue: https://github.com/protocolbuffers/protobuf/issues/6956
2. Protobuf JSON serialization uses the JSON support in `Google.Protobuf`. Its serializer is blocking (i.e. not async), which requires the input and output to be cached in memory so as not to block ASP.NET Core. Improvement would be to write a new serializer with the same behavior that is async and uses `System.Text.Json`.
3. `google/api/annotations.proto` and `google/api/http.proto` need to be added in the end-user's source code so the Protobuf compiler can load them along with the user's proto files. It would be a nicer developer experience if the user somehow didn't need to worry about those files.

### Experimental project

This project is experimental. It has known issues, it is not complete and it is not supported. We are interested in this technology but there is no commitment to completing it.

We want to gauge developer interest in gRPC+HTTP API. If gRPC+HTTP API is interesting to you then please give feedback.
