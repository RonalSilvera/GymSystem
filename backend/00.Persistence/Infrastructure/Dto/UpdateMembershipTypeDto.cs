namespace Infrastructure.Dto;

public class UpdateMembershipTypeDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
}
