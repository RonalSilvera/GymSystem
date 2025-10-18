using System;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Infrastructure.Factory;

public class AppDbContextFactory : IAppDbContextFactory
{
    private readonly ITenantConnectionProvider _tenantConnectionProvider;
    private readonly string _defaultTenant;

    public AppDbContextFactory(ITenantConnectionProvider tenantConnectionProvider, IConfiguration configuration)
    {
        _tenantConnectionProvider = tenantConnectionProvider;
        _defaultTenant = configuration.GetValue<string>("DefaultTenant") ?? "Tenant1";
    }

    public AppDbContext CreateDbContext(string tenantId)
    {
        var effectiveTenant = string.IsNullOrWhiteSpace(tenantId) ? _defaultTenant : tenantId;

        var connectionString = _tenantConnectionProvider.GetConnectionString(effectiveTenant);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException($"No se encontró cadena de conexión para el tenant '{effectiveTenant}'.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
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
}
