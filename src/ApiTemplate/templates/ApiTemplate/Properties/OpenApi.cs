using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi.Models;

[assembly: HostingStartup(typeof(OpenApiConfiguration))]

public class OpenApiConfiguration : IHostingStartup, IStartupFilter
{
    private static readonly string Version = "v1";
    private static readonly string DocumentPath = "openapi.json";
    private static readonly string UIPath = "docs";

    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            AddSwashbuckle(services, context.HostingEnvironment);
        });
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            ConfigureSwashbuckle(app);
            next(app);
        };
    }

    internal static void AddSwashbuckle(IServiceCollection services, IWebHostEnvironment hostingEnvironment)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(name: Version, info: new OpenApiInfo { Title = hostingEnvironment.ApplicationName, Version = Version });
        });
        services.AddTransient<IStartupFilter, OpenApiConfiguration>();
    }

    internal static void ConfigureSwashbuckle(IApplicationBuilder app)
    {
        var hostingEnvironment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        var rewriterOptions = new RewriteOptions();
        if (hostingEnvironment.IsDevelopment())
        {
            // Configure rules for Swagger UI
            // redirect from 'docs' to 'docs/'
            rewriterOptions.AddRedirect($"^{UIPath}$", $"{UIPath}/");
            // rewrite 'docs/' to 'docs/index.html'
            rewriterOptions.AddRewrite($"^{UIPath}/$", $"{UIPath}/index.html", skipRemainingRules: false);
            // rewrite 'docs/*' to 'swagger/*'
            rewriterOptions.AddRewrite($"^{UIPath}/(.+)$", $"swagger/$1", skipRemainingRules: true);
        }
        // Configure rules for Swagger docs
        // rewrite 'openapi.json' to 'swagger/{Version}/swagger.json'
        rewriterOptions.AddRewrite($"^{DocumentPath}$", $"swagger/{Version}/swagger.json", skipRemainingRules: true);
        app.UseRewriter(rewriterOptions);

        app.UseSwagger();

        if (hostingEnvironment.IsDevelopment())
        {
            app.UseSwaggerUI(options =>
            {
                // NOTE: The leading slash is *very* important in the document path below as the JS served
                //       attempts to workaround a relative path issue that breaks the UI without it
                options.SwaggerEndpoint($"/{DocumentPath}", $"{hostingEnvironment.ApplicationName} {Version}");
            });
        }
    }
}
