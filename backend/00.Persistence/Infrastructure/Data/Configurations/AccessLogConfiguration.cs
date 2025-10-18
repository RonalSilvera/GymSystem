using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class AccessLogConfiguration : IEntityTypeConfiguration<AccessLogs>
{
    public void Configure(EntityTypeBuilder<AccessLogs> builder)
    {
        builder.ToTable("AccessLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AccessTime).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.HasIndex(a => new { a.ClientId, a.AccessTime });
        builder.HasOne(a => a.Client)
            .WithMany(c => c.AccessLogs)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Status)
            .WithMany(s => s.AccessLogs)
            .HasForeignKey(a => a.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
