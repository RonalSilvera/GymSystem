namespace Infrastructure.Dto;

public class MembershipTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; }
}
