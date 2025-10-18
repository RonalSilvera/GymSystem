namespace Infrastructure.Dto;

public class ClientDto
{
    public int ClientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string RefId { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public int? MembershipTypeId { get; set; }
    public string? MembershipTypeName { get; set; }
    public int? MembershipStatusId { get; set; }
    public string? MembershipStatusName { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public decimal? AmountPaid { get; set; }
    public decimal? AmountDue { get; set; }
}
