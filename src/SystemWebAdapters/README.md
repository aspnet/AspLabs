# Microsoft.AspNetCore.SystemWebAdapters

This project provides a `System.Web.HttpContext` object with a subset of the APIs from `System.Web.dll` backed by `Microsoft.AspNetCore.Http` types. The goal of this is to enable developers who have taken a dependency on `HttpContext` in class libraries to be able to more quickly move to ASP.NET Core.

## Examples

A common use case that this is aimed at solving is in a project with multiple class libraries. Let's take a look at an example using the proposed adapters moving from .NET Framework to ASP.NET Core.

### ASP.NET Framework
Consider a controller that does something such as:

```cs
public class SomeController : Controller
{
  public ActionResult Index()
  {
    SomeOtherClass.SomeMethod(HttpContext.Current);
  }
}
```

which then has logic in a separate assembly passing that `HttpContext` around until finally, some inner method does some logic on it such as:

```cs
public class Class2
{
  public bool PerformSomeCheck(HttpContext context)
  {
    return context.Request.Headers["SomeHeader"] == "ExpectedValue";
  }
}
```

### ASP.NET Core

In order to run the above logic in ASP.NET Core, a developer will need to add the `Microsoft.AspNetCore.SystemWebAdapters` package, that will enable the projects to work on both platforms.

The libraries would need to be updated to understand the adapters, but it will be as simple as adding the package and recompiling. If these are the only dependencies a system has on `System.Web.dll`, then the libraries will be able to target .NET Standard to facillitate a simpler building process while migrating.

The controller in ASP.NET Core will now look like this:

```cs
public class SomeController : Controller
{
  [Route("/")]
  public IActionResult Index()
  {
    SomeOtherClass.SomeMethod(Context);
  }
}
```

Notice that since there's a `Controller.Context` property, they can pass that through, but it generally looks the same. Using implicit conversions, the `Microsoft.AspNetCore.Http.HttpContext` can be converted into the adapter that could then be passed around through the levels utilizing the code in the same way.

## Set up
Below are the steps needed to start using these adapters in your project:

1. Set up `NuGet.config` to point to the CI feed:
  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <configuration>
    <packageSources>
      <!--To inherit the global NuGet package sources remove the <clear/> line below -->
      <clear />
      <add key="nuget" value="https://api.nuget.org/v3/index.json" />
      <add key="dotnet6" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet6/nuget/v3/index.json" />
    </packageSources>
  </configuration>
  ```
2. Install `Microsoft.AspNetCore.SystemWebAdapters`
3. In your framework application:
   - The package installation will add a new handler to your web.config. This is to enable shared session state. If you will not need to use `HttpContext.Session`, feel free to remove this. Please see the section on [session state](#shared-session-state) to configure this
4. In your class libraries:
   - Class libraries can target .NET Standard 2.0 if desired which will ensure you are using the shared surface area
   - If you find that there's still some missing APIs, you may cross-compile with .NET Framework to maintain that behavior and handle it in .NET core in some other way
   - There should be no manual changes to enable using supported surface area of the adapters. If a member is not found, it is not currently supported on ASP.NET Core
5. For your ASP.NET Core application:
   - Register the adapter services:
    ```cs
    builder.Services.AddSystemWebAdapters();
    ``` 
   - Add the middleware after routing but before endpoints (if present);
   ```cs
   app.UseSystemWebAdapters();
   ```
   - For additional configuration, please see the [configuration](#configuration) section

## Usage
The ASP.NET Core implementation of `System.Web.HttpContext` attempts to bring behavior from ASP.NET framework, but can be configured. There is some behavior that can cause additional work to be done that may impact performance and memory usage that is configurable.

### Access `HttpContext`
Your code can operate on `System.Web.HttpContext` or `Microsoft.AspNetCore.Http.HttpContext`. This library provides implicit casting to each of these using a caching mechanism. For a given instance of `Microsoft.AspNetCore.Http.HttpContext`, implicit casting will always return the same `System.Web.HttpContext` instance. If you need a new instance (which will in turn create new instances of the request/response/and other objects), you may use the `HttpContext` constructor.

### `HttpContext.Request`
By default, the incoming request is not always seekable nor fully available. In order to get behavior seen in .NET Framework, you can opt into prebuffering the input stream. This will fully read the incoming stream and buffer it to memory or disk (depending on settings). 

This can be enabled by applying endpoint metadata that implements the `IPreBufferRequestStreamMetadata` interface. This is available as an attribute `PreBufferRequestStreamAttribute` that can be applied to controllers or methods.

To enable this on all MVC endpoints, there is an extension method that can be used as follows:

```cs
app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute()
        .PreBufferRequestStream();
});
```

### `HttpContext.Response`
In order to support behavior for `HttpContext.Response` that requires buffering the response before sending, endpoints must opt-into it with endpoint metadata implementing `IBufferResponseStreamMetadata`.

This enables APIs such as `HttpResponse.Output`, `HttpResponse.End()`, `HttpResponse.Clear()`, and `HttpResponse.SuppressContent`.

To enable this on all MVC endpoints, there is an extension method that can be used as follows:

```cs
app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute()
        .BufferResponseStream();
});
```

### Shared session state
In order to support `HttpContext.Session`, endpoints must opt-into it via metadata implementing `ISessionMetadata`.

To enable this on all MVC endpoints, there is an extension method that can be used as follows:

```cs
app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute()
        .RequireSystemWebAdapterSession();
});
```

This also requires some implementation of a session store. An initial implementation is being included that accesses a running ASP.NET Framework app and grabs session information from it. For details see [here](./docs/session-state/remote-session.md) for details.

## Supported Targets
- .NET Core App 3.1: This will implement the adapters against ASP.NET Core `HttpContext`. This will provide the following:
  - Conversions between ASP.NET Core `HttpContext` and `System.Web` adapter `HttpContext` (with appropriate caching so it will not cause perf hits for GC allocations)
  - Default implementations against `Microsoft.AspNetCore.Http.HttpContext`
  - Services that can be implemented to override some functionality such as session/caching/etc that may need to be customized to match experience.
- .NET Standard 2.0: This will essentially be a reference assembly. There will be no constructors for the types as ASP.NET Core will construct them based on their `HttpContext` and on framework there are already other constructors. However, this will allow class libraries to target .NET Standard instead of needing to multi-target which will then require everything it depends on to multi-target.
- .NET Framework 4.7.2: This will type forward the adapter classes to `System.Web` so that they can be unified and enable libraries built against .NET Standard 2.0 to run on .NET Framework instances.

## Known Limitations

Below are some of the limitations of the APIs in the adapters. These are usually due to building off of types used in ASP.NET Core that cannot be fully implemented in ASP.NET Core. In the future, analyzers may be used to flag usage to recommend better patterns.

- A number of APIs in `System.Web.HttpContext` are exposed as `NameValueCollection` instances. In order to reduce copying, many of these are implemented on ASP.NET Core using the core containers. This makes it so that for many of these collections, `Get(int)` (and any API that requires that such as `.Keys` or `.GetEnumerator()`) are unavailable as most of the containers in ASP.NET Core (such as `IHeaderDictionary`) does not have the ability to index by position.
