using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Clients>
{
    public void Configure(EntityTypeBuilder<Clients> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.DocumentNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(c => c.DocumentNumber).IsUnique();
        builder.Property(c => c.Email).HasMaxLength(150);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(255);
        builder.Property(c => c.RefId).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.RefId).IsUnique();
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.HasOne(c => c.Status)
            .WithMany(s => s.Clients)
            .HasForeignKey(c => c.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
