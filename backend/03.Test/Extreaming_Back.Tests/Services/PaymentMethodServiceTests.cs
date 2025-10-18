using BusinessLogic.Adapter.PaymentMethod;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.WorkUnit;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Xunit;

public class PaymentMethodServiceTests
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
    public async Task Create_AddsPaymentMethod()
    {
        var context = CreateContext(nameof(Create_AddsPaymentMethod));
        var service = new PaymentMethodService(context, new UnitOfWork(context));
        var dto = new CreatePaymentMethodDto { Name = "Cash" };
        var created = await service.CreateAsync(dto);
        Assert.Equal("Cash", created.Name);
        var saved = await context.PaymentMethods.FindAsync(created.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Create_DuplicateName_Throws()
    {
        var context = CreateContext(nameof(Create_DuplicateName_Throws));
        var service = new PaymentMethodService(context, new UnitOfWork(context));
        await service.CreateAsync(new CreatePaymentMethodDto { Name = "Cash" });
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreatePaymentMethodDto { Name = "Cash" }));
        Assert.Equal(HttpStatusCode.Conflict, ex.Status);
    }

    [Fact]
    public async Task Delete_SoftDeletes()
    {
        var context = CreateContext(nameof(Delete_SoftDeletes));
        var service = new PaymentMethodService(context, new UnitOfWork(context));
        var pm = await service.CreateAsync(new CreatePaymentMethodDto { Name = "Cash" });
        var result = await service.DeleteAsync(pm.Id);
        Assert.True(result);
        var entity = await context.PaymentMethods.FindAsync(pm.Id);
        Assert.False(entity!.Active);
    }
}
