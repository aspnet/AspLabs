## ASP.NET Core host for WebAPI2

This is a sample project that demonstrates hosting a WebAPI 2 app on ASP.NET Core.

#### Structure:
* src
  * System.Web.Http.AspNetCore - An ASP.NET Core middleware to execute WebAPI
  * System.Text.Json.Formatter - A MediaTypeFormatter based on System.Text.Json
* samples
  * SampleWebApiApp - A netstandard library containing WebApi controllers, filters, and handlers.
  * SampleAspNetCoreApp - A net5.0 ASP.NET Core app that hosts the WebAPI middleware.

#### Running the app:

* Grab a 5.0 compatible .NET SDK from https://get.dot.net
* Run the sample app

```
cd SampleAspNetCoreApp
dotnet run
```

* Visit the browser https://localhost:5001/api/values and see it do WebAPI
* Running in VS works just the same.
