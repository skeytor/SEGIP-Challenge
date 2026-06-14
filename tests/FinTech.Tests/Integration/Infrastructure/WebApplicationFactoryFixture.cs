using FinTech.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace FinTech.Tests.Integration.Infrastructure;

public sealed class WebApplicationFactoryFixture<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    private PostgreSqlContainer? _container;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddNpgsql<AppDbContext>($"{_container!.GetConnectionString()};Include Error Detail=true");
        });
        builder.UseEnvironment("Development");
    }

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:latest").Build();
        await _container.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container!.StopAsync();
        await base.DisposeAsync();
    }
}
