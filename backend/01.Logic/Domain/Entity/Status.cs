namespace Domain.Entity;

public class Status
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<Users> Users { get; set; } = new List<Users>();
    public ICollection<Clients> Clients { get; set; } = new List<Clients>();
    public ICollection<Memberships> Memberships { get; set; } = new List<Memberships>();
    public ICollection<Payments> Payments { get; set; } = new List<Payments>();
    public ICollection<AccessLogs> AccessLogs { get; set; } = new List<AccessLogs>();
}
