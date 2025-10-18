using Domain.Entity;

namespace Domain.Interface;

public interface IRoleRepository
{
    Task<IEnumerable<Roles>> All();
    Task<Roles?> Detail(Guid id);
    Task Create(Roles role);
    Task Update(Roles role);
    Task Delete(Roles role);
}
