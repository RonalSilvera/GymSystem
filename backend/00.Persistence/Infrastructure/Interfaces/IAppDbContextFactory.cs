using Infrastructure.Data;

namespace Infrastructure.Interfaces;

public interface IAppDbContextFactory
{
    AppDbContext CreateDbContext(string tenantId);
 
}