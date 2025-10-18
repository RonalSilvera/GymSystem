using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MembershipConfiguration : IEntityTypeConfiguration<Memberships>
{
    public void Configure(EntityTypeBuilder<Memberships> builder)
    {
        builder.ToTable("Memberships");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.Property(m => m.UpdatedAt).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.HasIndex(m => m.EndDate);
        builder.HasIndex(m => new { m.ClientId, m.StatusId });
        builder.HasOne(m => m.Client)
            .WithMany(c => c.Memberships)
            .HasForeignKey(m => m.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MembershipType)
            .WithMany(mt => mt.Memberships)
            .HasForeignKey(m => m.MembershipTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Status)
            .WithMany(s => s.Memberships)
            .HasForeignKey(m => m.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
