# The below content is now obsolete. Please refer to Blazor documentation if you'd like to use Custom Elements
## The previously experimental Microsoft.AspNetCore.Components.CustomElements package for building standards based custom elements with Blazor is no longer experimental and is now part of [.NET 7](https://devblogs.microsoft.com/dotnet/asp-net-core-updates-in-dotnet-7-preview-6/#blazor-custom-elements-no-longer-experimental). You can learn more about how to use Custom Elements in Blazor from our [documentation](https://learn.microsoft.com/aspnet/core/blazor/components#blazor-custom-elements)


# Blazor Custom Elements

This package provides a simple mechanism for using Blazor components as custom elements. This means you can easily render Blazor components dynamically from other SPA frameworks, such as Angular or React.

## Running the Angular sample

Clone this repo. Then, in a command prompt, execute:

 * `cd samples/BlazorAppProvidingCustomElements`
 * `dotnet watch`

Leave that running, and open a second command prompt, and execute:

 * `cd samples/angular-app-with-blazor`
 * `npm install`
 * `npm start`

Now when you browse to http://localhost:4200/, you'll see an Angular application that dynamically renders Blazor WebAssembly components, passing parameters to them.

## Running the React sample

Clone this repo. Then, in a command prompt, execute:

 * `cd samples/BlazorAppProvidingCustomElements`
 * `dotnet watch`

Leave that running, and open a second command prompt, and execute:

 * `cd samples/react-app-with-blazor`
 * `yarn install`
 * `yarn start`

Now when you browse to http://localhost:3000/, you'll see a React application that dynamically renders Blazor WebAssembly components, passing parameters to them.

## Adding this to your own project

1. Start with or create a Blazor WebAssembly or Blazor Server project that contains your Blazor components
2. Install the NuGet package `Microsoft.AspNetCore.Components.CustomElements`
3. In your Blazor application startup configuration, remove any normal Blazor root components, and instead register components as custom elements. See [Example for Blazor WebAssembly](#webassembly-example) or [Example for Blazor Server](#server-example).
4. Configure your external SPA framework application to serve the Blazor framework files and render your Blazor custom elements. For example, see [Configuring Angular](#configuring-angular) or [Configuring React](#configuring-react). Similar techniques will work with other SPA frameworks.

### WebAssembly example

For Blazor WebAssembly, remove lines like this from `Program.cs`:

```cs
builder.RootComponents.Add<App>("#root");
```

... and add lines like the following instead:

```cs
builder.RootComponents.RegisterAsCustomElement<Counter>("my-blazor-counter");
builder.RootComponents.RegisterAsCustomElement<ProductGrid>("product-grid");
```

### Server example

For Blazor Server, in `Program.cs` or `Startup.cs`, change the call to `AddServerSideBlazor` to pass an `options` callback like the following:

```cs
builder.Services.AddServerSideBlazor(options =>
{
    options.RootComponents.RegisterAsCustomElement<Counter>("my-blazor-counter");
    options.RootComponents.RegisterAsCustomElement<ProductGrid>("product-grid");
});
```

### Configuring Angular

1. In your Angular `index.html`, add the following at the end of `<body>`:

    ```html
    <script src="_content/Microsoft.AspNetCore.Components.CustomElements/BlazorCustomElements.js"></script>
    <script src="_framework/blazor.webassembly.js"></script>
    ```

    Note that the tag for `BlazorCustomElements.js` is only needed temporarily. This step will be removed in a future update.

2. Run the application now and see that the browser gets a 404 error for those two JavaScript files. To fix this, [configure Angular CLI to proxy unmatches requests to the ASP.NET Core development server](https://angular.io/guide/build#proxying-to-a-backend-server). Of course, you also need to be running your ASP.NET Core development server at the same time for this to work.

3. In any of your Angular components, render the custom elements corresponding to your Blazor components. For example, add to a `template` markup like the following:

    ```html
    <my-blazor-counter [attr.title]="counter.title"></my-blazor-counter>
    ```

    This assumes that your component declares a parameter like the following:

    ```cs
    [Parameter] public string Title { get; set; }
    ```

    If you have parameters that are complex-typed objects, you can use Angular's property-setting syntax as follows:

    ```html
    <my-blazor-counter [someComplexObject]="someJsObject"></my-blazor-counter>
    ```

    This will cause `someJsObject` to be JSON-serialized and supplied as a Blazor component parameter called `SomeComplexObject`.

4. If you get a compiler error saying that your custom element is "not a known element", [add `CUSTOM_ELEMENTS_SCHEMA` to your Angular component or module](https://stackoverflow.com/a/40407697).

### Configuring React

1. In your React `index.html`, add the following at the end of `<body>`:

    ```html
    <script src="_content/Microsoft.AspNetCore.Components.CustomElements/BlazorCustomElements.js"></script>
    <script src="_framework/blazor.webassembly.js"></script>
    ```

    Note that the tag for `BlazorCustomElements.js` is only needed temporarily. This step will be removed in a future update.

2. Run the application now and see that the browser gets a 404 error for those two JavaScript files. To fix this, [configure React's development server to proxy unmatches requests to the ASP.NET Core development server](https://create-react-app.dev/docs/proxying-api-requests-in-development/). Of course, you also need to be running your ASP.NET Core development server at the same time for this to work.

3. In any of your React components, render the custom elements corresponding to your Blazor components. For example, have a React component render output include:

    ```html
    <my-blazor-counter title={title} increment-amount={incrementAmount}></my-blazor-counter>
    ```

    Whenever you edit your React component source code, you'll see it update automatically in the browser without losing the state within your Blazor component. The two hot reload systems can coexist and cooperate.

### Publishing your combined application

The easiest way to have an application with both .NET parts and a 3rd-party SPA framework is to use the ASP.NET Core project templates [for Angular](https://docs.microsoft.com/aspnet/core/client-side/spa/angular) or [for React](https://docs.microsoft.com/aspnet/core/client-side/spa/react). This will automatically gather the files required for the combined application during publishing.

If you're not using one of those project templates, you'll need to publish both the .NET and JavaScript parts separately and manually combine the files into a single deployable set.

## Passing parameters

You can pass parameters to your Blazor component either as HTML attributes or as JavaScript properties on the DOM element.

For example, if your component declares a parameters like the following:

```cs
[Parameter] public int IncrementAmount { get; set; }
```

... then you can pass a value as an HTML attribute follows:

```html
<my-blazor-counter increment-amount="123"></my-blazor-counter>
```

Notice that the attribute name is kebab-case (i.e., `increment-amount`, not `IncrementAmount`).

Alternatively, you can set it as a JavaScript property on the element object:

```js
const elem = document.querySelector("my-blazor-counter");
elem.incrementAmount = 123;
```

Notice that the property name is camelCase (i.e., `incrementAmount`, not `IncrementAmount`).

You can update parameter values at any time using either attribute or property syntax.

### Supported parameter types

 * Using JavaScript property syntax, you can pass objects of any JSON-serializable type
 * Using HTML attributes, you are limited to passing objects of string, boolean, or numerical types
