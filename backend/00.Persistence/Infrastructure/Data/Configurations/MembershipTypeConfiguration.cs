using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class MembershipTypeConfiguration : IEntityTypeConfiguration<MembershipTypes>
{
    public void Configure(EntityTypeBuilder<MembershipTypes> builder)
    {
        builder.ToTable("MembershipTypes");
        builder.HasKey(mt => mt.Id);
        builder.Property(mt => mt.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(mt => mt.Name).IsUnique();
        builder.Property(mt => mt.Description).HasMaxLength(255);
        builder.Property(mt => mt.Price).HasColumnType("decimal(10,2)");
        builder.Property(mt => mt.Active).HasDefaultValue(true);
        builder.HasCheckConstraint("CK_MembershipTypes_DurationDays", "`DurationDays` > 0");
        builder.HasCheckConstraint("CK_MembershipTypes_Price", "`Price` >= 0");
    }
}
