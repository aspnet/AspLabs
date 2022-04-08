using System.Web.Adapters;

var builder = WebApplication.CreateBuilder();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSystemWebAdapters()
    .AddRemoteAppSession(options =>
    {
        options.RemoteAppUrl = new Uri("https://localhost:44339/fallback/session-state");

        ClassLibrary.SessionUtils.RegisterSessionKeys(options);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSystemWebAdapters();

app.UseEndpoints(endpoints =>
{
    app.MapDefaultControllerRoute()
        .RequireSystemWebAdapterSession();

    app.MapReverseProxy();
});

app.Run();
