using Infrastructure.Data;
using Persistence.Repository.Base;
using Microsoft.EntityFrameworkCore;
using Domain.Interface;

namespace Repository.User;

public class UserRepository(AppDbContext context)
    : Repository<Domain.Entity.Users>(context), IUserRepository
{
    public new async Task<IEnumerable<Domain.Entity.Users>> All()
    {
        return await base.All();
    }

    public new async Task<Domain.Entity.Users?> Detail(Guid id)
    {
        return await base.Detail(id);
    }

    public new async Task Create(Domain.Entity.Users user)
    {
        await base.Create(user);
    }

    public new async Task Update(Domain.Entity.Users user)
    {
        await base.Update(user);
    }

    public new async Task Delete(Domain.Entity.Users user)
    {
        await base.Delete(user);
    }

    public async Task<Domain.Entity.Users?> FindByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
