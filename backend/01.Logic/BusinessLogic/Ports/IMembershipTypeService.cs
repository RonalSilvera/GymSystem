using Infrastructure.Dto;

namespace BusinessLogic.Ports;

public interface IMembershipTypeService
{
    Task<IEnumerable<MembershipTypeDto>> ListAsync();
    Task<MembershipTypeDto?> DetailAsync(int id);
    Task<MembershipTypeDto> CreateAsync(CreateMembershipTypeDto dto);
    Task<MembershipTypeDto?> UpdateAsync(int id, UpdateMembershipTypeDto dto);
    Task<bool> DeleteAsync(int id);
}
