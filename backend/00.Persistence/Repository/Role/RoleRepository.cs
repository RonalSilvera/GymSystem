using Infrastructure.Data;
using Persistence.Repository.Base;
using Microsoft.EntityFrameworkCore;
using Domain.Interface;

namespace Repository.Role;

public class RoleRepository(AppDbContext context)
    : Repository<Domain.Entity.Roles>(context), IRoleRepository
{
    public new async Task<IEnumerable<Domain.Entity.Roles>> All()
    {
        return await base.All();
    }

    public new async Task<Domain.Entity.Roles?> Detail(Guid id)
    {
        return await base.Detail(id);
    }

    public new async Task Create(Domain.Entity.Roles role)
    {
        await base.Create(role);
    }

    public new async Task Update(Domain.Entity.Roles role)
    {
        await base.Update(role);
    }

    public new async Task Delete(Domain.Entity.Roles role)
    {
        await base.Delete(role);
    }
}
