using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Okidoki.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".microsoft",
                    "usersecrets",
                    "b7799fc0-2b85-46c9-8b71-e275c6dbde37",
                    "secrets.json"),
                optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("AppDb")
            ?? throw new InvalidOperationException("ConnectionStrings:AppDb must be configured in user-secrets or environment variables.");

        var sqlConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            MultipleActiveResultSets = true,
        }.ConnectionString;

        optionsBuilder.UseSqlServer(sqlConnectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
