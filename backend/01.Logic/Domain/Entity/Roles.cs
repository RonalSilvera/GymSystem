namespace Domain.Entity;

public class Roles
{
    public Guid RoleId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<UserRole> UserRole { get; set; } = new List<UserRole>();

}