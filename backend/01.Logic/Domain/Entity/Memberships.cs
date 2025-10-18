namespace Domain.Entity;

public class Memberships
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int MembershipTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Clients Client { get; set; } = null!;
    public MembershipTypes MembershipType { get; set; } = null!;
    public Status Status { get; set; } = null!;
    public ICollection<Payments> Payments { get; set; } = new List<Payments>();
}
