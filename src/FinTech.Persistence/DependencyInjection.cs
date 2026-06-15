using FinTech.Domain.Repositories;
using FinTech.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.UnitOfWork;

namespace FinTech.Persistence;

public static class DependencyInjection
{
    private const string ConnectionStringName = "DefaultConnection";
    public static IServiceCollection AddPersistence(this IServiceCollection services) =>
        services
            .AddDbContext<AppDbContext>((sp, options) =>
            {
                IConfiguration config = sp.GetRequiredService<IConfiguration>();

                // Railway injects DATABASE_URL as postgresql://user:pass@host:port/db
                // Npgsql needs it as a standard connection string
                string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                string? connectionString = databaseUrl is not null
                    ? ConvertDatabaseUrl(databaseUrl)
                    : config.GetConnectionString(ConnectionStringName);

                options.UseNpgsql(connectionString);
            })
            .AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>())
            .AddRepositories();

    private static string ConvertDatabaseUrl(string databaseUrl)
    {
        // postgresql://user:password@host:port/database
        var uri = new Uri(databaseUrl);
        string[] userInfo = uri.UserInfo.Split(':');
        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }

    internal static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
            .AddScoped<ILoanRepository, LoanRepository>()
            .AddScoped<ITransactionRepository, TransactionRepository>();
}
