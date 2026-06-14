namespace FinTech.API.Extensions;

internal static class CorsExtension
{
    public static IHostApplicationBuilder AddDefaultCorsPolicy(this IHostApplicationBuilder app)
    {
        app.Services.AddCors(options =>
            options.AddPolicy(CorsPolicyNames.WebClient, policy =>
            {
                string[] allowedOrigins = app.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();

            }));
        return app;
    }
}

internal static class CorsPolicyNames
{
    public const string WebClient = "WebClient";
}