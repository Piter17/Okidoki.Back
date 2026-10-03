using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Okidoki.Infrastructure.Data;
using Testcontainers.MsSql;

namespace Okidoki.Api.Tests;


public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    const string sqlContainer = "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04";
    private readonly MsSqlContainer container = new MsSqlBuilder(sqlContainer)
        .WithDatabase("It")
        .WithPortBinding(1555)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.Converters.Clear();
            });
        });
        builder.UseSetting("ConnectionStrings:AppDb", container.GetConnectionString());
        builder.UseSetting("ConnectionStrings:BlobStorage", "UseDevelopmentStorage=true");
    }

    public async ValueTask InitializeAsync()
    {
        await container.StartAsync();
        await using var scope = Services.CreateAsyncScope();

        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await database.Database.EnsureDeletedAsync();
        await database.Database.MigrateAsync();
    }

    public override async ValueTask DisposeAsync ()
    {
        await container.DisposeAsync();
        await base.DisposeAsync();
    }
}