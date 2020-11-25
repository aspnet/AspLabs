## WebAPI -> ASP.NET Core Migration

This project is an attempt to provide guidance and tooling to help migrate from ASP.NET to ASP.NET Core. As a start, we're looking at helping migrate from WebAPI to ASP.NET Core by automating some of the boiler plate work, providing build time warning for behavior that does not carry forward to ASP.NET Core, and shims that lets you compile, and possibly test, intermediate code that's been migrated over.

### Steps

* Create a new .NET Core API project (`dotnet new api`)
* Add a refrence to the shim (`./samples/CompatShim/MinimalCompatShim.csproj`) and the Migrator.
* Install package references such as Azure SDK packages, 3rd party logging or DI containers, etc that are currently used by your project.
* From your WebAPI project, copy over code to the new app.
    - Ideally you should be able to refactor your data models, services, business logic, i.e. things not related to HTTP in to a netstandard library that is shared between the WebAPI and ASP.NET Core app. This gives you the ability to continue hosting both apps while making sure changes do not have to replicated between the two as you migrate.
    - Do not copy over code for Global.asax, `App_Start`, project files,  since a lot of these concepts are different or missing in ASP.NET Core. It would be much easier to approach these from scratch.
* One way to be able to validate your work as you progress through the migration is by tackling one controller at a time. Copy over enough assets required to compile a single controller, work through build errors and use code fixes to resolve warnings. At the end of it, you might be able to test your controller running as part of the new app.
    - Copy a constructor over.
    - Resolve any missing namespace errors, and remove any namespaces that are no longer available. Typically, you will need to add the `Microsoft.AspNetCore.Mvc` namespace to each controller file and remove `System.Web.Http.*` namespaces.
    - `Control + .` is your friend. Use Quick Actions in Visual Studio to perform this sort of work.
    - Some APIs might have the same names, but new signatures. Use the API browser https://docs.microsoft.com/en-us/dotnet/api/?view=aspnetcore-5.0 to learn more about these APIs. Searching for these terms in the conceptual might also help. For e.g. https://docs.microsoft.com/en-us/search/?scope=ASP.NET%20Core&view=aspnetcore-5.0&terms=Cors&category=Documentation should bring up all the conceptual doc related to CORS in .NET Core.


* At the end of doing this, start thinking about removing the use of the CompatShim. The CompatShim provides types that allows your WebAPI controllers to compile using ASP.NET Core. However, these types aren't innate to ASP.NET Core and are thus aren't recommended as a long term solution. Removing it would be ideal to ensure your app is highly compatible with ASP.NET Core. To do this, turn on analyzers labeled <BLAH> in VS. (Show how analyzers are enabled). Now tackle the new set of build warnings. At the end of it, your app should be ready to remove the shim.


