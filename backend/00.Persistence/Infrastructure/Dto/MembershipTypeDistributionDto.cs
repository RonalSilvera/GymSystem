namespace Infrastructure.Dto;

public class MembershipTypeDistributionDto
{
    public int MembershipTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}
