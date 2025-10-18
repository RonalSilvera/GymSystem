using System.Linq;
using BusinessLogic.Adapter.Payment;
using Domain.Entity;
using Infrastructure.Data;
using Infrastructure.WorkUnit;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class PaymentServiceTests
{
    private static AppDbContext CreateContext(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task Create_WithCoupon_AppliesDiscountAndIncrementsUse()
    {
        var context = CreateContext(nameof(Create_WithCoupon_AppliesDiscountAndIncrementsUse));
        var now = DateTime.UtcNow;
        context.Clients.Add(new Clients { FullName = "John", DocumentNumber = "123", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        context.Coupons.Add(new Coupons { Code = "OFF10", Percent = 10, Active = true });
        await context.SaveChangesAsync();
        var service = new PaymentService(context, new UnitOfWork(context));
        var receipt = await service.CreateAsync(1, 2, 1, null, "OFF10", null, now);
        var payment = await context.Payments.FindAsync(receipt.PaymentId);
        Assert.Equal(6000m, payment!.DiscountAmount);
        var coupon = await context.Coupons.FirstAsync(c => c.Code == "OFF10");
        Assert.Equal(1, coupon.UsedCount);
    }

    [Fact]
    public async Task Refund_MarksOriginalInactiveAndCreatesNegativePayment()
    {
        var context = CreateContext(nameof(Refund_MarksOriginalInactiveAndCreatesNegativePayment));
        var now = DateTime.UtcNow;
        context.Clients.Add(new Clients { FullName = "Jane", DocumentNumber = "456", RefId = "R2", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        await context.SaveChangesAsync();
        var service = new PaymentService(context, new UnitOfWork(context));
        var receipt = await service.CreateAsync(1, 2, 1, null, null, null, now);
        var refund = await service.RefundAsync(receipt.PaymentId, "error", now.AddMinutes(1));
        var original = await context.Payments.FindAsync(receipt.PaymentId);
        var refundPayment = await context.Payments.FindAsync(refund.RefundId);
        Assert.Equal(2, original!.StatusId);
        Assert.Equal(-original.Amount, refundPayment!.Amount);
        Assert.Equal(original.Id, refundPayment.ParentPaymentId);
    }

    [Fact]
    public async Task Create_WithPendingMembership_ActivatesIt()
    {
        var context = CreateContext(nameof(Create_WithPendingMembership_ActivatesIt));
        var now = DateTime.UtcNow;
        context.Clients.Add(new Clients { Id = 1, FullName = "John", DocumentNumber = "123", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        var pending = new Memberships { Id = 1, ClientId = 1, MembershipTypeId = 1, StartDate = now, EndDate = now.AddDays(1), StatusId = 5, CreatedAt = now, UpdatedAt = now };
        context.Memberships.Add(pending);
        await context.SaveChangesAsync();
        var service = new PaymentService(context, new UnitOfWork(context));
        var receipt = await service.CreateAsync(1, 1, 1, null, null, null, now.AddMinutes(1));
        var membership = await context.Memberships.FindAsync(pending.Id);
        Assert.Equal(1, membership!.StatusId);
        Assert.Equal(pending.Id, receipt.MembershipId);
    }

    [Fact]
    public async Task CreatePartial_UpdatesMembershipStatusAndTotals()
    {
        var context = CreateContext(nameof(CreatePartial_UpdatesMembershipStatusAndTotals));
        var now = DateTime.UtcNow;
        context.Clients.Add(new Clients { Id = 1, FullName = "John", DocumentNumber = "123", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        await context.SaveChangesAsync();
        var service = new PaymentService(context, new UnitOfWork(context));
        var receipt = await service.CreatePartialAsync(1, 2, 1, 30000m, null, now);
        var membership = await context.Memberships.FindAsync(receipt.MembershipId);
        Assert.Equal(now.AddDays(30).Date, membership!.EndDate.Date);
        Assert.Equal(6, membership.StatusId);
        var payment = await context.Payments.FindAsync(receipt.PaymentId);
        Assert.Equal(30000m, payment!.Amount);
        Assert.Equal(membership.Id, payment.MembershipId);
        Assert.Equal(30000m, receipt.TotalPaid);
    }

    [Fact]
    public async Task CreatePartial_PaysRemainingBalanceAndMarksAsPaid()
    {
        var context = CreateContext(nameof(CreatePartial_PaysRemainingBalanceAndMarksAsPaid));
        var now = DateTime.UtcNow;
        context.Clients.Add(new Clients { Id = 1, FullName = "John", DocumentNumber = "123", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        await context.SaveChangesAsync();
        var service = new PaymentService(context, new UnitOfWork(context));

        await service.CreatePartialAsync(1, 2, 1, 30000m, null, now);
        var second = await service.CreatePartialAsync(1, 2, 1, 30000m, null, now.AddMinutes(10));

        var membership = await context.Memberships.FindAsync(second.MembershipId);
        Assert.Equal(7, membership!.StatusId);
        Assert.Equal(60000m, second.TotalPaid);
        var payments = await context.Payments.Where(p => p.MembershipId == membership.Id && p.StatusId == 1).ToListAsync();
        Assert.Equal(2, payments.Count);
    }

    [Fact]
    public async Task CreatePartial_ForShortMembership_Throws()
    {
        var context = CreateContext(nameof(CreatePartial_ForShortMembership_Throws));
        var now = DateTime.UtcNow;
        context.Clients.Add(new Clients { Id = 1, FullName = "John", DocumentNumber = "123", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        await context.SaveChangesAsync();
        var service = new PaymentService(context, new UnitOfWork(context));

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePartialAsync(1, 1, 1, 5000m, null, now));
    }
}
