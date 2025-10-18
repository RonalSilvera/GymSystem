namespace Domain.Entity;

public class Users
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public int StatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Status Status { get; set; } = null!;
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
