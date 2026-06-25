using FinTech.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FinTech.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Adds all implementations of ICommandHandler and IQueryHandler from the FinTech.Application assembly to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationUseCases(this IServiceCollection services)
    {
        Assembly assembly = typeof(DependencyInjection).Assembly;

        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Impl = t, Interface = i })
            .Where(x => x.Interface.IsGenericType &&
                        (x.Interface.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
                         x.Interface.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

        foreach (var type in types)
        {
            services.AddScoped(type.Interface, type.Impl);
        }

        return services;
    }
}
