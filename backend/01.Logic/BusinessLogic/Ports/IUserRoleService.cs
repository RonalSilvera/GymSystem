using Domain.Entity;
namespace BusinessLogic.Ports;

public interface IUserRoleService
{
    Task<IEnumerable<UserRole>> All();
    Task<UserRole?> Detail(Guid id);
    Task Create(Guid userId, Guid roleId);
    Task Update(UserRole userRole);
    Task Delete(Guid id);
}