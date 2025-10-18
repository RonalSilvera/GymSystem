using AutoMapper;
using BusinessLogic.Ports;
using Domain.Interface;
using Infrastructure.Interfaces;
using Infrastructure.Dto;

namespace BusinessLogic.Adapter.Role;

public class RoleService(IRoleRepository repository, IUnitOfWork unitOfWork, IMapper mapper) : IRoleService
{
    private readonly IRoleRepository _repository = repository;
    private readonly IUnitOfWork _uow = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<RoleDto> Create(RoleDto role)
    {
        var entity = _mapper.Map<Domain.Entity.Roles>(role);
        await _repository.Create(entity);
        await _uow.Complete();
        return _mapper.Map<RoleDto>(entity);
    }

    public async Task<IEnumerable<RoleDto>> All()
    {
        var list = await _repository.All();
        return _mapper.Map<IEnumerable<RoleDto>>(list);
    }

    public async Task<RoleDto?> Detail(Guid id)
    {
        var entity = await _repository.Detail(id);
        return entity is null ? null : _mapper.Map<RoleDto>(entity);
    }

    public async Task Update(RoleDto role)
    {
        var entity = _mapper.Map<Domain.Entity.Roles>(role);
        await _repository.Update(entity);
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
