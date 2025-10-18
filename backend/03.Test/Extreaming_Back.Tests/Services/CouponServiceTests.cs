using BusinessLogic.Adapter.Coupon;
using Domain.Entity;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.WorkUnit;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Xunit;

public class CouponServiceTests
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
    public async Task Create_AddsCoupon()
    {
        var context = CreateContext(nameof(Create_AddsCoupon));
        var service = new CouponService(context, new UnitOfWork(context));
        var dto = new CreateCouponDto { Code = "OFF10", Percent = 10 };
        var created = await service.CreateAsync(dto);
        Assert.Equal("OFF10", created.Code);
        var saved = await context.Coupons.FindAsync(created.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Create_DuplicateCode_Throws()
    {
        var context = CreateContext(nameof(Create_DuplicateCode_Throws));
        var service = new CouponService(context, new UnitOfWork(context));
        await service.CreateAsync(new CreateCouponDto { Code = "OFF10", Percent = 10 });
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateCouponDto { Code = "OFF10", Percent = 5 }));
        Assert.Equal(HttpStatusCode.Conflict, ex.Status);
    }

    [Fact]
    public async Task Delete_SoftDeletes()
    {
        var context = CreateContext(nameof(Delete_SoftDeletes));
        var service = new CouponService(context, new UnitOfWork(context));
        var c = await service.CreateAsync(new CreateCouponDto { Code = "OFF10", Percent = 10 });
        var result = await service.DeleteAsync(c.Id);
        Assert.True(result);
        var entity = await context.Coupons.FindAsync(c.Id);
        Assert.False(entity!.Active);
    }
}
