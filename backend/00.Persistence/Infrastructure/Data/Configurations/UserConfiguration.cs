using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasCheckConstraint("CK_Users_Role", "`Role` IN ('SuperAdmin','Admin','Operator')");

        builder.Property(u => u.PasswordHash)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(255);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.Property(u => u.UpdatedAt)
            .HasDefaultValueSql("UTC_TIMESTAMP()");

        builder.HasOne(u => u.Status)
            .WithMany(s => s.Users)
            .HasForeignKey(u => u.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
