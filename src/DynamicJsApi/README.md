## Blazor Dynamic JS API
An API allowing JavaScipt-like interactions with JavaScript objects, all in a .NET context.

### Example:
```csharp
private async Task OnClick()
{
    // Get scoped access to the browser's 'window' JS object
    await using var window = JSRuntime.GetDynamicWindow();

    // Use JS-like property access and method invocation
    window.console.log("Hello, world!");

    // Create bindings to other JS objects
    var element = document.getElementById("some-element-id");

    // Mutate JS objects with ease
    element.innerText = "Not a single line of JS :)";

    // Even create your own JS objects!
    var myObject = JSObject.Create(window, new
    {
        numValue = 42,
        strValue = "37",
        objValue = new
        {
            boolValue = true
        }
    });

    // Looks like JS, acts like JS, but isn't JS (this operation is illegal in normal C#!)
    var result = myObject.numValue - myObject.strValue; // 42 - "37" = 5

    // ...
}
```

## Why?
One of the greatest aspects of Blazor is that it enables developers to create web apps without using JS. However, there are times where the best solution to a problem is to use JS in your Blazor app, even if it's a few simple lines. Instead of introducing a new JS file, linking it up, and calling its functions via `IJSRuntime`, you can write a few lines of C# using the Dynamic JS API.

## Setup
1. Add a package reference to `Microsoft.AspNetCore.DynamicJS`.
2. Add `<script src="_content/Microsoft.AspNetCore.DynamicJS/dynamicJsApi.js"></script>` to your `index.html` when using Blazor WebAssembly, or to your `_Host.cshtml` on Blazor Server.

## Usage
Before using this package, the crucial thing to understand is that all dynamic JS operations are lazily evaluated. This is done to minimize the number of JS interop calls, maximizing perforamance. There are only two conditions that trigger a dynamic JS evaluation:
1. A `JSObject` is evaluated as another .NET type. Various ways this can be done are discussed later.
2. The "root" `JSObject` is disposed. The `using` statement is a common pattern to make sure disposal occurs, but you can also dispose the root manually. This will be discussed in more detail later.

In the example at the top of this document, no JS is executed until the end of `OnClick()`, since a `JSObject` is never converted to another .NET type. This means that it only takes a *single* JS interop calls to execute all that code. Awesome!

### Evaluating a `JSObject` as another .NET type
Let's say we wanted to get `window.document.title` as a .NET `string`. This can be done via the static method `JSObject.EvaluateAsync()`:

```csharp
await using var window = JSRuntime.GetDynamicWindow();
var title = await JSObject.EvaluateAsync<string>(window.document.title);
```

Note that if we were to simply say:
```csharp
var title = window.document.title;
```
then `title` would be another `JSObject`, not a `string`, and no evaluation would occur.

**IMPORTANT**: Attempting to cast a `JSObject` to another .NET type, **either implicitly or explicitly**, will throw an exception if the root `JSObject` was obtained via `GetDynamicWindow()`, because dynamic JS evaluations must occur asynchronously. If you're on Blazor server, there's no better alternative, but if you're on Blazor WebAssembly, read the next section.

### Synchronous lazy evaluation
If you're using Blazor WebAssembly, JS interop calls can be done in-process (i.e. synchronously). For this reason, it's possible to evaluate a `JSObject` by directly casting it to a .NET type (either implicitly or explicitly), as long as you create the root `JSObject` with `IJSInProcessRuntime.GetInProcessWindowDynamic()`. For example:
```csharp
using var window = JSInProcessRuntime.GetInProcessWindowDynamic();

// Explicit cast, triggering an evaluation
var title = (string)window.document.title;

// Implicit cast to a .NET bool, again triggering an evaluation
if (document.getElementById("done-loading"))
{
    // Do something
}
```

Compared to manually evaluating asynchronously, this is far less verbose. Again, note that the example above will only run on Blazor WebAssembly.

### Cleaning up
The root `JSObject` (the one obtained directly from `IJSRuntime`) should be disposed to free up internal memory. There are two ways to do this:
1. Create the root `JSObject` with the `using` statement pattern as shown in the examples in this document. Make sure to use `await using` when using asynchronous evaluation.
2. Call `DisposeAsync()` on the root `JSObject` when using asynchronous evaluation, or `Dispose()` when using synchronous evaluation.
