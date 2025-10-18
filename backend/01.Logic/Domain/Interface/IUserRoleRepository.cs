using Domain.Entity;

namespace Domain.Interface;

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> All();
    Task<UserRole?> Detail(Guid id);
    Task Create(UserRole userRole);
    Task Update(UserRole userRole);
    Task Delete(UserRole userRole);
}
