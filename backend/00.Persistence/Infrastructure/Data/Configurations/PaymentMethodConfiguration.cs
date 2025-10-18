using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethods>
{
    public void Configure(EntityTypeBuilder<PaymentMethods> builder)
    {
        builder.ToTable("PaymentMethods");
        builder.HasKey(pm => pm.Id);
        builder.Property(pm => pm.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(pm => pm.Name).IsUnique();
        builder.Property(pm => pm.Active).HasDefaultValue(true);
    }
}
