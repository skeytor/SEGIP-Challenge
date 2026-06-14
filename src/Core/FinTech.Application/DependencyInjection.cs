using FinTech.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace FinTech.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUseCases(this IServiceCollection services) =>
        services
            .AddScoped<ILoanService, LoanService>();
}
