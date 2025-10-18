namespace Domain.Entity;

public class MembershipTypes
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; }

    public ICollection<Memberships> Memberships { get; set; } = new List<Memberships>();
}
