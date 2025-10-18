using Domain.Entity;

namespace Domain.Interface;

public interface IUserRepository
{
    Task<IEnumerable<Users>> All();
    Task<Users?> Detail(Guid id);
    Task Create(Users user);
    Task Update(Users user);
    Task Delete(Users user);
    Task<Users?> FindByEmail(string email);
}
