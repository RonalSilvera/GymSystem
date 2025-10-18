using BusinessLogic.Adapter.Access;
using BusinessLogic.Ports;
using Domain.Entity;
using Infrastructure.Data;
using Infrastructure.WorkUnit;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class AccessServiceTests
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
    public async Task Validate_Allows_WhenMembershipActive()
    {
        var context = CreateContext(nameof(Validate_Allows_WhenMembershipActive));
        SeedActiveMembership(context);
        var service = new AccessService(context, new UnitOfWork(context));

        var result = await service.ValidateAsync("REF1", null, DateTime.UtcNow);

        Assert.True(result.Allowed);
    }

    [Fact]
    public async Task Validate_Denies_WhenMembershipExpired()
    {
        var context = CreateContext(nameof(Validate_Denies_WhenMembershipExpired));
        SeedExpiredMembership(context);
        var service = new AccessService(context, new UnitOfWork(context));

        var result = await service.ValidateAsync("REF1", null, DateTime.UtcNow);

        Assert.False(result.Allowed);
        Assert.Equal("vencido", result.Reason);
    }

    [Fact]
    public async Task Validate_Allows_WhenMembershipPartial()
    {
        var context = CreateContext(nameof(Validate_Allows_WhenMembershipPartial));
        SeedPartialMembership(context);
        var service = new AccessService(context, new UnitOfWork(context));

        var result = await service.ValidateAsync("REF1", null, DateTime.UtcNow);

        Assert.True(result.Allowed);
    }

    [Fact]
    public async Task Validate_Denies_WhenCooldown()
    {
        var context = CreateContext(nameof(Validate_Denies_WhenCooldown));
        SeedActiveMembership(context);
        var service = new AccessService(context, new UnitOfWork(context));

        var now = DateTime.UtcNow;
        var first = await service.ValidateAsync("REF1", null, now);
        Assert.True(first.Allowed);

        var second = await service.ValidateAsync("REF1", null, now.AddMinutes(1));

        Assert.False(second.Allowed);
        Assert.Equal("cooldown", second.Reason);
    }

    [Fact]
    public async Task ListLogs_FiltersByAllowed()
    {
        var context = CreateContext(nameof(ListLogs_FiltersByAllowed));
        SeedActiveMembership(context);
        var service = new AccessService(context, new UnitOfWork(context));
        var now = DateTime.UtcNow;
        await service.ValidateAsync("REF1", null, now);
        await service.ValidateAsync("REF1", null, now.AddMinutes(1));

        var allowed = await service.ListLogsAsync(null, null, null, true, 1, 10);
        Assert.All(allowed.Items, l => Assert.True(l.Allowed));

        var denied = await service.ListLogsAsync(null, null, null, false, 1, 10);
        Assert.All(denied.Items, l => Assert.False(l.Allowed));
    }

    private static void SeedActiveMembership(AppDbContext context)
    {
        context.Clients.Add(new Clients
        {
            Id = 1,
            FullName = "Client",
            DocumentNumber = "123",
            RefId = "REF1",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.Memberships.Add(new Memberships
        {
            Id = 1,
            ClientId = 1,
            MembershipTypeId = 1,
            StartDate = DateTime.UtcNow.AddMinutes(-10),
            EndDate = DateTime.UtcNow.AddDays(1),
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }

    private static void SeedExpiredMembership(AppDbContext context)
    {
        context.Clients.Add(new Clients
        {
            Id = 1,
            FullName = "Client",
            DocumentNumber = "123",
            RefId = "REF1",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        });
        context.Memberships.Add(new Memberships
        {
            Id = 1,
            ClientId = 1,
            MembershipTypeId = 1,
            StartDate = DateTime.UtcNow.AddDays(-2),
            EndDate = DateTime.UtcNow.AddDays(-1),
            StatusId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        });
        context.SaveChanges();
    }

    private static void SeedPartialMembership(AppDbContext context)
    {
        context.Clients.Add(new Clients
        {
            Id = 1,
            FullName = "Client",
            DocumentNumber = "123",
            RefId = "REF1",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.Memberships.Add(new Memberships
        {
            Id = 1,
            ClientId = 1,
            MembershipTypeId = 1,
            StartDate = DateTime.UtcNow.AddMinutes(-10),
            EndDate = DateTime.UtcNow.AddDays(1),
            StatusId = 6,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }
}
