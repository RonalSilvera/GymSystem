using Infrastructure.Dto;

namespace BusinessLogic.Ports;

public interface IUserService
{
    Task<IEnumerable<UserDto>> All();
    Task<UserDto?> Detail(Guid id);
    Task<UserDto> Create(UserDto user);
    Task<UserDto?> Update(Guid id, UpdateUserDto user);
    Task Delete(Guid id);
}
