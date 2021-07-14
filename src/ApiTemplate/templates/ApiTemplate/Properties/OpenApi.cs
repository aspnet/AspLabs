using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi.Models;

[assembly:HostingStartup(typeof(OpenApiConfiguration))]

public class OpenApiConfiguration : IHostingStartup, IStartupFilter
{
    private static readonly string Version = "v1";
    private static readonly string DocumentName = "openapi.json";
    private static readonly string UIPath = "docs";

    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices((context, services) =>
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(DocumentName, new OpenApiInfo { Title = context.HostingEnvironment.ApplicationName, Version = Version });
            });
            services.AddTransient<IStartupFilter, OpenApiConfiguration>();
        });
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            next(app);
            ConfigureSwashbuckle(app);
        };
    }

    private void ConfigureSwashbuckle(IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        if (env.IsDevelopment())
        {
            // Fixup for Swashbuckle RoutePrefix issue
            var rewriterOptions = new RewriteOptions();
            // redirect from 'docs' to 'docs/'
            rewriterOptions.AddRedirect($"^{UIPath}$", $"{UIPath}/");
            // rewrite 'docs/' to 'docs/index.html'
            rewriterOptions.AddRewrite($"^{UIPath}/$", $"{UIPath}/index.html", skipRemainingRules: true);
            app.UseRewriter(rewriterOptions);

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/{DocumentName}", $"{env.ApplicationName} {Version}");
                options.RoutePrefix = UIPath;
            });
        }

        // This has to be last as the route template is SUPER greedy (it matches '/anything')
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "/{documentName}";
        });
    }
}