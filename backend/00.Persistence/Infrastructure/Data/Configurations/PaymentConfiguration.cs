using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payments>
{
    public void Configure(EntityTypeBuilder<Payments> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasColumnType("decimal(10,2)");
        builder.Property(p => p.DiscountAmount).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        builder.Property(p => p.InvoiceNumber);
        builder.Property(p => p.CouponCode).HasMaxLength(100);
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("UTC_TIMESTAMP()");
        builder.HasIndex(p => p.PaymentDate);
        builder.HasIndex(p => p.InvoiceNumber).IsUnique();
        builder.HasOne(p => p.Client)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Membership)
            .WithMany(m => m.Payments)
            .HasForeignKey(p => p.MembershipId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.PaymentMethod)
            .WithMany(pm => pm.Payments)
            .HasForeignKey(p => p.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Status)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Coupon)
            .WithMany(c => c.Payments)
            .HasForeignKey(p => p.CouponCode)
            .HasPrincipalKey(c => c.Code)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.ParentPayment)
            .WithMany()
            .HasForeignKey(p => p.ParentPaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
