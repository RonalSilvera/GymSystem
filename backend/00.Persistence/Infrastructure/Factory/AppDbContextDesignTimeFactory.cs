using System;
using System.IO;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Infrastructure.Factory;

public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var configuration = BuildConfiguration();

        var defaultTenant = configuration["DefaultTenant"] ?? "Tenant1";
        var connectionString =
            configuration.GetConnectionString(defaultTenant) ??
            configuration.GetConnectionString("Tenant1") ??
            "Server=localhost;Port=3306;Database=gymsystemmanagement;User=root;Password=;SslMode=None;AllowPublicKeyRetrieval=True;";

        var serverVersion = DetectServerVersion(connectionString);

        optionsBuilder.UseMySql(connectionString, serverVersion, mySqlOptions =>
        {
            mySqlOptions.MigrationsHistoryTable(AppDbContext.MigrationsHistoryTableName);
            mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            mySqlOptions.CommandTimeout(120);
        });

        return new AppDbContext(optionsBuilder.Options);
    }

    private static ServerVersion DetectServerVersion(string connectionString)
    {
        try
        {
            return ServerVersion.AutoDetect(connectionString);
        }
        catch (MySqlConnector.MySqlException)
        {
            return MySqlServerVersion.LatestSupportedServerVersion;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Unable to connect", StringComparison.OrdinalIgnoreCase))
        {
            return MySqlServerVersion.LatestSupportedServerVersion;
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        var basePath = Directory.GetCurrentDirectory();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile(Path.Combine("02.Service", "API", "appsettings.json"), optional: true)
            .AddJsonFile(Path.Combine("02.Service", "API", "appsettings.Development.json"), optional: true)
            .AddJsonFile(Path.Combine("02.Service", "API", "appsettings.Local.json"), optional: true)
            .AddEnvironmentVariables(prefix: "GYMSYSTEM_")
            .Build();
    }
}
