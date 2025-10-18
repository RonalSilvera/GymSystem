namespace Domain.Entity;

public class UserRole
{
    public Guid UserRoleId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    public Users Users { get; set; } = null!;
    public Roles Role { get; set; } = null!;

}