using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupons>
{
    public void Configure(EntityTypeBuilder<Coupons> builder)
    {
        builder.ToTable("Coupons");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Code).IsUnique();
        builder.Property(c => c.Description).HasMaxLength(255);
        builder.Property(c => c.DiscountAmount).HasColumnType("decimal(10,2)");
        builder.Property(c => c.Percent).HasColumnType("decimal(5,2)");
        builder.Property(c => c.UsedCount).HasDefaultValue(0);
        builder.Property(c => c.Active).HasDefaultValue(true);
        builder.HasCheckConstraint("CK_Coupons_Discount_XOR", "((`DiscountAmount` IS NULL) XOR (`Percent` IS NULL))");
        builder.HasCheckConstraint("CK_Coupons_Percent_Range", "(`Percent` IS NULL) OR (`Percent` >= 0 AND `Percent` <= 100)");
    }
}
