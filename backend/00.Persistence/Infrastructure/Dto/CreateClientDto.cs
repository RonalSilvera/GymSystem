namespace Infrastructure.Dto;

public class CreateClientDto
{
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string RefId { get; set; } = string.Empty;
    public int? MembershipTypeId { get; set; }
}
