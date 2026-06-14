using FinTech.Domain.Repositories;
using FinTech.Persistence.Repositories;
using FinTech.Domain.Entities;
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
                string? connectionString = config.GetConnectionString(ConnectionStringName);
                options.UseNpgsql(connectionString);
            })
            .AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>())
            .AddRepositories();

    internal static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services
            .AddScoped(typeof(IRepository<,>), typeof(Repository<,>))
            .AddScoped<ILoanRepository, LoanRepository>()
            .AddScoped<ITransactionRepository, TransactionRepository>();
}
