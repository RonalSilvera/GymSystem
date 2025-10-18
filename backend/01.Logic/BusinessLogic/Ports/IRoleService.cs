using Infrastructure.Dto;
namespace BusinessLogic.Ports;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> All();
    Task<RoleDto?> Detail(Guid id);
    Task<RoleDto> Create(RoleDto roles);
    Task Update(RoleDto role);
    Task Delete(Guid id);
}