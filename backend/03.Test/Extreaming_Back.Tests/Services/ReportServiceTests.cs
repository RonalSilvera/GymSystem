using BusinessLogic.Adapter.Report;
using Domain.Entity;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ReportServiceTests
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
    public async Task Revenue_Sums_PerDay()
    {
        var ctx = CreateContext(nameof(Revenue_Sums_PerDay));
        var now = new DateTime(2024, 1, 1);
        ctx.Payments.AddRange(
            new Payments { Id = 1, ClientId = 1, MembershipId = 1, PaymentMethodId = 1, Amount = 100, PaymentDate = now, StatusId = 1, CreatedAt = now, UpdatedAt = now },
            new Payments { Id = 2, ClientId = 1, MembershipId = 1, PaymentMethodId = 1, Amount = 50, PaymentDate = now, StatusId = 1, CreatedAt = now, UpdatedAt = now },
            new Payments { Id = 3, ClientId = 1, MembershipId = 1, PaymentMethodId = 1, Amount = 75, PaymentDate = now.AddDays(1), StatusId = 1, CreatedAt = now, UpdatedAt = now }
        );
        ctx.SaveChanges();
        var service = new ReportService(ctx);
        var report = await service.GetRevenueAsync(now, now.AddDays(1));
        Assert.Equal(2, report.DailyTotals.Count);
        Assert.Equal(150, report.DailyTotals.First().Total);
        Assert.Equal(225, report.GrandTotal);
    }

    [Fact]
    public async Task Attendance_ReturnsCounts()
    {
        var ctx = CreateContext(nameof(Attendance_ReturnsCounts));
        ctx.Clients.AddRange(
            new Clients { Id = 1, FullName = "A", DocumentNumber = "1", RefId = "R1", StatusId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Clients { Id = 2, FullName = "B", DocumentNumber = "2", RefId = "R2", StatusId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        var baseTime = new DateTime(2024, 1, 1, 10, 0, 0);
        ctx.AccessLogs.AddRange(
            new AccessLogs { Id = 1, ClientId = 1, AccessTime = baseTime, StatusId = 1 },
            new AccessLogs { Id = 2, ClientId = 1, AccessTime = baseTime.AddMinutes(30), StatusId = 1 },
            new AccessLogs { Id = 3, ClientId = 2, AccessTime = baseTime.AddHours(1), StatusId = 1 }
        );
        ctx.SaveChanges();
        var service = new ReportService(ctx);
        var result = await service.GetAttendanceAsync(baseTime.AddHours(-1), baseTime.AddHours(2), 5);
        Assert.Equal(2, result.ByHour.Count);
        Assert.Contains(result.TopClients, c => c.ClientId == 1 && c.Count == 2);
    }

    [Fact]
    public async Task Expiring_ReturnsMemberships()
    {
        var ctx = CreateContext(nameof(Expiring_ReturnsMemberships));
        var now = DateTime.UtcNow;
        ctx.Clients.Add(new Clients { Id = 1, FullName = "A", DocumentNumber = "1", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        ctx.Memberships.Add(new Memberships { Id = 1, ClientId = 1, MembershipTypeId = 1, StartDate = now.AddDays(-1), EndDate = now.AddDays(3), StatusId = 1, CreatedAt = now, UpdatedAt = now });
        ctx.SaveChanges();
        var service = new ReportService(ctx);
        var result = await service.GetExpiringAsync(now, 7);
        Assert.Single(result);
    }

    [Fact]
    public async Task Expiring_IncludesPartialMemberships()
    {
        var ctx = CreateContext(nameof(Expiring_IncludesPartialMemberships));
        var now = DateTime.UtcNow;
        ctx.Clients.Add(new Clients { Id = 1, FullName = "A", DocumentNumber = "1", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        ctx.Memberships.Add(new Memberships { Id = 1, ClientId = 1, MembershipTypeId = 1, StartDate = now.AddDays(-1), EndDate = now.AddDays(2), StatusId = 6, CreatedAt = now, UpdatedAt = now });
        ctx.SaveChanges();
        var service = new ReportService(ctx);
        var result = await service.GetExpiringAsync(now, 7);
        Assert.Single(result);
    }

    [Fact]
    public async Task TopMemberships_ReturnsOrdered()
    {
        var ctx = CreateContext(nameof(TopMemberships_ReturnsOrdered));
        var now = DateTime.UtcNow;
        ctx.Clients.Add(new Clients { Id = 1, FullName = "A", DocumentNumber = "1", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now });
        ctx.Memberships.AddRange(
            new Memberships { Id = 1, ClientId = 1, MembershipTypeId = 1, StartDate = now, EndDate = now.AddDays(1), StatusId = 1, CreatedAt = now, UpdatedAt = now },
            new Memberships { Id = 2, ClientId = 1, MembershipTypeId = 2, StartDate = now, EndDate = now.AddDays(30), StatusId = 1, CreatedAt = now, UpdatedAt = now }
        );
        ctx.Payments.AddRange(
            new Payments { Id = 1, ClientId = 1, MembershipId = 1, PaymentMethodId = 1, Amount = 10, PaymentDate = now, StatusId = 1, CreatedAt = now, UpdatedAt = now },
            new Payments { Id = 2, ClientId = 1, MembershipId = 2, PaymentMethodId = 1, Amount = 50, PaymentDate = now, StatusId = 1, CreatedAt = now, UpdatedAt = now },
            new Payments { Id = 3, ClientId = 1, MembershipId = 2, PaymentMethodId = 1, Amount = 50, PaymentDate = now, StatusId = 1, CreatedAt = now, UpdatedAt = now }
        );
        ctx.SaveChanges();
        var service = new ReportService(ctx);
        var list = await service.GetTopMembershipsAsync(now.AddDays(-1), now.AddDays(1), 5);
        Assert.Equal(2, list.Count);
        Assert.Equal(2, list.First().MembershipTypeId);
    }
}
