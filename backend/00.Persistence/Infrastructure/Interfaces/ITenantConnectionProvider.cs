namespace Infrastructure.Interfaces;

public interface ITenantConnectionProvider
{
    string GetConnectionString(string tenantId);
}