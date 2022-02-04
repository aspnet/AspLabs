# System.Web.Adapters

`System.Web.HttpContext` adapters backed by `Microsoft.AspNetCore.Http` types. The goal of this is to enable developers who have taken a dependency on `HttpContext` in class libraries to be able to more quickly move to ASP.NET Core. These adapters will present a subset of the surface area as `System.Web.HttpContext` and what can be accessed on it.

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

In order to run the above logic in ASP.NET Core, a developer will need to add the `System.Web.Adapters` package, that will enable the projects to work on both platforms.

The libraries would need to be updated to understand the adapters, but it will be as simple as adding the package and recompiling. If these are the only dependencies a system has on `System.Web`, then the libraries will be able to target .NET Standard to facillitate a simpler building process while migrating.

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

## Supported Targets
- .NET Core App 3.1: This will implement the adapters against ASP.NET Core `HttpContext`. This will provide the following:
  - Conversions between ASP.NET Core `HttpContext` and `System.Web` adapter `HttpContext` (with appropriate caching so it will not cause perf hits for GC allocations)
  - Default implementations against `Microsoft.AspNetCore.Http.HttpContext`
  - Services that can be implemented to override some functionality such as session/caching/etc that may need to be customized to match experience.
- .NET Standard 2.0: This will essentially be a reference assembly. There will be no constructors for the types as ASP.NET Core will construct them based on their `HttpContext` and on framework there are already other constructors. However, this will allow class libraries to target .NET Standard instead of needing to multi-target which will then require everything it depends on to multi-target.
- .NET Framework 4.7.2: This will type forward the adapter classes to `System.Web` so that they can be unified and enable libraries built against .NET Standard 2.0 to run on .NET Framework instances.
