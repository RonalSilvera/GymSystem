using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.TenantConnection;

public class TenantConnectionProvider(IConfiguration configuration) : ITenantConnectionProvider
{
    public string GetConnectionString(string tenantId)
    {
        var connectionString = configuration.GetConnectionString(tenantId);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception($"No se encontró la cadena de conexión para el tenant '{tenantId}'.");
        }

        return connectionString;
    }
}