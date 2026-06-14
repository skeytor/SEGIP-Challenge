using FinTech.Persistence;
using FinTech.Persistence.Seeders;
using Microsoft.EntityFrameworkCore;

namespace FinTech.API.Extensions;

internal static class MigrationExtensions
{
    public static void ApplyMigrations(this IHost app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        AppDbContext ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ctx.Database.Migrate();
        DbSeeder.Seed(ctx);
    }
}
