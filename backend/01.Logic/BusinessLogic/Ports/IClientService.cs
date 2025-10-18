using Infrastructure.Dto;

namespace BusinessLogic.Ports;

public interface IClientService
{
    Task<(IEnumerable<ClientDto> Items, int Total, int Active, int Inactive)> ListAsync(string? search, int? statusId, int page, int pageSize);
    Task<ClientDto?> DetailAsync(int id);
    Task<ClientDto> CreateAsync(CreateClientDto dto, DateTime now);
    Task<ClientDto?> UpdateAsync(int id, UpdateClientDto dto, DateTime now);
    Task<bool> SoftDeleteAsync(int id, DateTime now);
    Task<bool> ReactivateAsync(int id, DateTime now);
}
