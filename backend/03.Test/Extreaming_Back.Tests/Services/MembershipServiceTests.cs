using BusinessLogic.Adapter.Membership;
using Domain.Entity;
using Infrastructure.Data;
using Infrastructure.WorkUnit;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Xunit;

public class MembershipServiceTests
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

    private static void SeedClient(AppDbContext context)
    {
        context.Clients.Add(new Clients { Id = 1, FullName = "Client", DocumentNumber = "123", RefId = "REF1", StatusId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        context.SaveChanges();
    }

    [Fact]
    public async Task ActivateOrRenew_NoMembership_CreatesNew()
    {
        var context = CreateContext(nameof(ActivateOrRenew_NoMembership_CreatesNew));
        SeedClient(context);
        var service = new MembershipService(context, new UnitOfWork(context));
        var now = DateTime.UtcNow;

        var result = await service.ActivateOrRenewAsync(1, 1, null, now);
        var membership = await context.Memberships.FirstAsync();
        Assert.Equal(1, context.Memberships.Count());
        Assert.Equal("Activo", result.Status);
        Assert.Equal(membership.Id, result.MembershipId);
    }

    [Fact]
    public async Task ActivateOrRenew_Expired_CreatesNew()
    {
        var context = CreateContext(nameof(ActivateOrRenew_Expired_CreatesNew));
        SeedClient(context);
        var past = DateTime.UtcNow.AddDays(-10);
        context.Memberships.Add(new Memberships
        {
            Id = 1,
            ClientId = 1,
            MembershipTypeId = 1,
            StartDate = past.AddDays(-5),
            EndDate = past.AddDays(-1),
            StatusId = 1,
            CreatedAt = past.AddDays(-5),
            UpdatedAt = past.AddDays(-5)
        });
        context.SaveChanges();
        var service = new MembershipService(context, new UnitOfWork(context));
        var now = DateTime.UtcNow;

        var result = await service.ActivateOrRenewAsync(1, 1, null, now);

        var previous = await context.Memberships.FirstAsync(m => m.Id == 1);
        Assert.Equal(3, previous.StatusId);
        Assert.Equal("Activo", result.Status);
    }

    [Fact]
    public async Task ActivateOrRenew_Active_ChangeType_ClosesPrevious()
    {
        var context = CreateContext(nameof(ActivateOrRenew_Active_ChangeType_ClosesPrevious));
        SeedClient(context);
        var now = DateTime.UtcNow;
        context.Memberships.Add(new Memberships
        {
            Id = 1,
            ClientId = 1,
            MembershipTypeId = 1,
            StartDate = now.AddDays(-5),
            EndDate = now.AddDays(5),
            StatusId = 1,
            CreatedAt = now.AddDays(-5),
            UpdatedAt = now.AddDays(-5)
        });
        context.SaveChanges();
        var service = new MembershipService(context, new UnitOfWork(context));

        var result = await service.ActivateOrRenewAsync(1, 2, null, now);

        var previous = await context.Memberships.FirstAsync(m => m.Id == 1);
        Assert.Equal(2, previous.StatusId);
        Assert.True(previous.EndDate <= now);
        Assert.Equal("Activo", result.Status);
        Assert.Equal(2, result.MembershipId);
    }

    [Fact]
    public async Task ActivateOrRenew_Active_SameType_ReturnsExisting()
    {
        var context = CreateContext(nameof(ActivateOrRenew_Active_SameType_ReturnsExisting));
        SeedClient(context);
        var now = DateTime.UtcNow;
        context.Memberships.Add(new Memberships
        {
            Id = 1,
            ClientId = 1,
            MembershipTypeId = 1,
            StartDate = now.AddDays(-5),
            EndDate = now.AddDays(5),
            StatusId = 1,
            CreatedAt = now.AddDays(-5),
            UpdatedAt = now.AddDays(-5)
        });
        context.SaveChanges();
        var service = new MembershipService(context, new UnitOfWork(context));

        var result = await service.ActivateOrRenewAsync(1, 1, null, now);

        Assert.Equal(1, context.Memberships.Count());
        Assert.Equal(1, result.MembershipId);
        Assert.Equal("Activo", result.Status);
    }
}
