using Infrastructure.Data;
using Persistence.Repository.Base;
using Microsoft.EntityFrameworkCore;
using Domain.Interface;

namespace Repository.UserRole;

public class UserRoleRepository(AppDbContext context)
    : Repository<Domain.Entity.UserRole>(context), IUserRoleRepository
{
    public async Task<IEnumerable<Domain.Entity.UserRole>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync();
    }

    public new async Task<IEnumerable<Domain.Entity.UserRole>> All()
    {
        return await base.All();
    }

    public new async Task<Domain.Entity.UserRole?> Detail(Guid id)
    {
        return await base.Detail(id);
    }

    public new async Task Create(Domain.Entity.UserRole userRole)
    {
        await base.Create(userRole);
    }

    public new async Task Update(Domain.Entity.UserRole userRole)
    {
        await base.Update(userRole);
    }

    public new async Task Delete(Domain.Entity.UserRole userRole)
    {
        await base.Delete(userRole);
    }
}
