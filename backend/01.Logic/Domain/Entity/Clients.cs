namespace Domain.Entity;

public class Clients
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string RefId { get; set; } = null!;
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Status Status { get; set; } = null!;
    public ICollection<Memberships> Memberships { get; set; } = new List<Memberships>();
    public ICollection<Payments> Payments { get; set; } = new List<Payments>();
    public ICollection<AccessLogs> AccessLogs { get; set; } = new List<AccessLogs>();
}
