using BusinessLogic.Ports;
using Domain.Entity;
using Domain.Interface;
using Infrastructure.Interfaces;

namespace BusinessLogic.Adapter.UserRole;

public class UserRoleService(IUserRoleRepository repository, IUnitOfWork unitOfWork) : IUserRoleService
{
    private readonly IUserRoleRepository _repository = repository;
    private readonly IUnitOfWork _uow = unitOfWork;

    public async Task<IEnumerable<Domain.Entity.UserRole>> All()
    {
        return await _repository.All();
    }

    public async Task<Domain.Entity.UserRole?> Detail(Guid id)
    {
        return await _repository.Detail(id);
    }

    public async Task Create(Guid userId, Guid roleId)
    {
        var entity = new Domain.Entity.UserRole { UserRoleId = Guid.NewGuid(), UserId = userId, RoleId = roleId };
        await _repository.Create(entity);
        await _uow.Complete();
    }

    public async Task Update(Domain.Entity.UserRole userRole)
    {
        await _repository.Update(userRole);
        await _uow.Complete();
    }

    public async Task Delete(Guid id)
    {
        var entity = await _repository.Detail(id);
        if (entity != null)
        {
            await _repository.Delete(entity);
            await _uow.Complete();
        }
    }
}
