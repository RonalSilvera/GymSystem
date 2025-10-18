using BusinessLogic.Adapter.Client;
using Domain.Entity;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.WorkUnit;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Linq;
using Xunit;

public class ClientServiceTests
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
    public async Task Create_AddsClient()
    {
        var context = CreateContext(nameof(Create_AddsClient));
        var service = new ClientService(context, new UnitOfWork(context));
        var dto = new CreateClientDto { FullName = "John", DocumentNumber = "123", RefId = "R1" };
        var now = DateTime.UtcNow;
        var created = await service.CreateAsync(dto, now);
        Assert.Equal("John", created.FullName);
        var saved = await context.Clients.FindAsync(created.ClientId);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Create_WithMembership_AssignsMembership()
    {
        var context = CreateContext(nameof(Create_WithMembership_AssignsMembership));
        var service = new ClientService(context, new UnitOfWork(context));
        var mType = new MembershipTypes { Name = "Basic", DurationDays = 30, Price = 10m, Active = true };
        await context.MembershipTypes.AddAsync(mType);
        await context.SaveChangesAsync();
        var now = DateTime.UtcNow;
        var dto = new CreateClientDto { FullName = "John", DocumentNumber = "123", RefId = "R1", MembershipTypeId = mType.Id };
        var created = await service.CreateAsync(dto, now);
        var membership = await context.Memberships.FirstOrDefaultAsync(m => m.ClientId == created.ClientId);
        Assert.NotNull(membership);
        Assert.Equal(mType.Id, membership!.MembershipTypeId);
        Assert.Equal(mType.Id, created.MembershipTypeId);
        Assert.Equal(mType.Name, created.MembershipTypeName);
    }

    [Fact]
    public async Task Create_DuplicateDocument_Throws()
    {
        var context = CreateContext(nameof(Create_DuplicateDocument_Throws));
        var service = new ClientService(context, new UnitOfWork(context));
        var now = DateTime.UtcNow;
        await service.CreateAsync(new CreateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1" }, now);
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateClientDto { FullName = "B", DocumentNumber = "123", RefId = "R2" }, now));
        Assert.Equal(HttpStatusCode.Conflict, ex.Status);
    }

    [Fact]
    public async Task Update_AssignsMembership_PendingStatus()
    {
        var context = CreateContext(nameof(Update_AssignsMembership_PendingStatus));
        var service = new ClientService(context, new UnitOfWork(context));
        var mType = new MembershipTypes { Name = "Basic", DurationDays = 30, Price = 10m, Active = true };
        await context.MembershipTypes.AddAsync(mType);
        await context.SaveChangesAsync();
        var now = DateTime.UtcNow;
        var created = await service.CreateAsync(new CreateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1" }, now);
        var dto = new UpdateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1", MembershipTypeId = mType.Id };
        var updated = await service.UpdateAsync(created.ClientId, dto, now.AddMinutes(1));
        var membership = await context.Memberships.FirstOrDefaultAsync(m => m.ClientId == created.ClientId);
        Assert.Equal(5, membership!.StatusId);
        Assert.Equal(5, updated!.MembershipStatusId);
        Assert.Equal(mType.Id, updated.MembershipTypeId);
        Assert.Equal(mType.Name, updated.MembershipTypeName);
    }

    [Fact]
    public async Task Update_ClearMembership_DisablesExisting()
    {
        var context = CreateContext(nameof(Update_ClearMembership_DisablesExisting));
        var service = new ClientService(context, new UnitOfWork(context));
        var mType = new MembershipTypes { Name = "Basic", DurationDays = 30, Price = 10m, Active = true };
        await context.MembershipTypes.AddAsync(mType);
        await context.SaveChangesAsync();
        var now = DateTime.UtcNow;
        var created = await service.CreateAsync(new CreateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1", MembershipTypeId = mType.Id }, now);
        var membership = await context.Memberships.FirstAsync(m => m.ClientId == created.ClientId);
        membership.StatusId = 1;
        await context.SaveChangesAsync();
        var dto = new UpdateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1", ClearMembership = true };
        var updated = await service.UpdateAsync(created.ClientId, dto, now.AddMinutes(1));
        var updatedMembership = await context.Memberships.FindAsync(membership.Id);
        Assert.Equal(2, updatedMembership!.StatusId);
        Assert.Null(updated!.MembershipStatusId);
        Assert.Null(updated.MembershipTypeId);
        Assert.Null(updated.MembershipTypeName);
    }

    [Fact]
    public async Task Update_ChangeMembership_ReplacesExisting()
    {
        var context = CreateContext(nameof(Update_ChangeMembership_ReplacesExisting));
        var service = new ClientService(context, new UnitOfWork(context));
        var basic = new MembershipTypes { Name = "Basic", DurationDays = 30, Price = 10m, Active = true };
        var premium = new MembershipTypes { Name = "Premium", DurationDays = 60, Price = 20m, Active = true };
        await context.MembershipTypes.AddRangeAsync(basic, premium);
        await context.SaveChangesAsync();
        var now = DateTime.UtcNow;
        var created = await service.CreateAsync(new CreateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1", MembershipTypeId = basic.Id }, now);
        var membership = await context.Memberships.FirstAsync(m => m.ClientId == created.ClientId);
        membership.StatusId = 1;
        await context.SaveChangesAsync();
        var dto = new UpdateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1", MembershipTypeId = premium.Id };
        var updated = await service.UpdateAsync(created.ClientId, dto, now.AddMinutes(1));
        var memberships = context.Memberships.Where(m => m.ClientId == created.ClientId).ToList();
        Assert.Equal(2, memberships.Count);
        var oldMembership = memberships.First(m => m.MembershipTypeId == basic.Id);
        var newMembership = memberships.First(m => m.MembershipTypeId == premium.Id);
        Assert.Equal(2, oldMembership.StatusId);
        Assert.Equal(5, newMembership.StatusId);
        Assert.Equal(premium.Id, updated!.MembershipTypeId);
        Assert.Equal(premium.Name, updated.MembershipTypeName);
    }

    [Fact]
    public async Task SoftDelete_SetsInactiveStatus()
    {
        var context = CreateContext(nameof(SoftDelete_SetsInactiveStatus));
        var service = new ClientService(context, new UnitOfWork(context));
        var now = DateTime.UtcNow;
        var created = await service.CreateAsync(new CreateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1" }, now);
        var result = await service.SoftDeleteAsync(created.ClientId, now.AddMinutes(1));
        Assert.True(result);
        var client = await context.Clients.FindAsync(created.ClientId);
        Assert.Equal(2, client!.StatusId);
    }

    [Fact]
    public async Task SoftDelete_DisablesMembershipsAndPayments()
    {
        var context = CreateContext(nameof(SoftDelete_DisablesMembershipsAndPayments));
        var service = new ClientService(context, new UnitOfWork(context));
        var now = DateTime.UtcNow;

        var membershipType = new MembershipTypes { Name = "Basic", DurationDays = 30, Price = 10m, Active = true };
        await context.MembershipTypes.AddAsync(membershipType);
        var paymentMethod = new PaymentMethods { Name = "Cash", Active = true };
        await context.PaymentMethods.AddAsync(paymentMethod);
        await context.SaveChangesAsync();

        var client = await service.CreateAsync(new CreateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1" }, now);

        var membership = new Memberships
        {
            ClientId = client.ClientId,
            MembershipTypeId = membershipType.Id,
            StartDate = now,
            EndDate = now.AddDays(30),
            StatusId = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        await context.Memberships.AddAsync(membership);
        await context.SaveChangesAsync();

        var payment = new Payments
        {
            ClientId = client.ClientId,
            MembershipId = membership.Id,
            PaymentMethodId = paymentMethod.Id,
            Amount = 10m,
            InvoiceNumber = 1,
            PaymentDate = now,
            StatusId = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        await service.SoftDeleteAsync(client.ClientId, now.AddMinutes(1));

        var updatedMembership = await context.Memberships.FindAsync(membership.Id);
        var updatedPayment = await context.Payments.FindAsync(payment.Id);

        Assert.Equal(2, updatedMembership!.StatusId);
        Assert.Equal(now.AddMinutes(1), updatedMembership.EndDate);
        Assert.Equal(2, updatedPayment!.StatusId);
    }

    [Fact]
    public async Task Reactivate_SetsClientActive_WithoutActivatingMemberships()
    {
        var context = CreateContext(nameof(Reactivate_SetsClientActive_WithoutActivatingMemberships));
        var service = new ClientService(context, new UnitOfWork(context));
        var now = DateTime.UtcNow;

        var membershipType = new MembershipTypes { Name = "Basic", DurationDays = 30, Price = 10m, Active = true };
        await context.MembershipTypes.AddAsync(membershipType);
        var paymentMethod = new PaymentMethods { Name = "Cash", Active = true };
        await context.PaymentMethods.AddAsync(paymentMethod);
        await context.SaveChangesAsync();

        var client = await service.CreateAsync(new CreateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1" }, now);
        var membership = new Memberships
        {
            ClientId = client.ClientId,
            MembershipTypeId = membershipType.Id,
            StartDate = now,
            EndDate = now.AddDays(30),
            StatusId = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        await context.Memberships.AddAsync(membership);
        await context.SaveChangesAsync();
        var payment = new Payments
        {
            ClientId = client.ClientId,
            MembershipId = membership.Id,
            PaymentMethodId = paymentMethod.Id,
            Amount = 10m,
            InvoiceNumber = 1,
            PaymentDate = now,
            StatusId = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        await context.Payments.AddAsync(payment);
        await context.SaveChangesAsync();

        await service.SoftDeleteAsync(client.ClientId, now.AddMinutes(1));
        var reactivated = await service.ReactivateAsync(client.ClientId, now.AddMinutes(2));

        Assert.True(reactivated);
        var updatedClient = await context.Clients.FindAsync(client.ClientId);
        var updatedMembership = await context.Memberships.FindAsync(membership.Id);
        var updatedPayment = await context.Payments.FindAsync(payment.Id);

        Assert.Equal(1, updatedClient!.StatusId);
        Assert.Equal(2, updatedMembership!.StatusId);
        Assert.Equal(now.AddMinutes(1), updatedMembership.EndDate);
        Assert.Equal(2, updatedPayment!.StatusId);
    }

    [Fact]
    public async Task List_IncludesMembershipStatus()
    {
        var context = CreateContext(nameof(List_IncludesMembershipStatus));
        var service = new ClientService(context, new UnitOfWork(context));
        var now = DateTime.UtcNow;
        var membershipType = new MembershipTypes { Name = "Basic", DurationDays = 30, Price = 10m, Active = true };
        await context.MembershipTypes.AddAsync(membershipType);
        await context.SaveChangesAsync();

        var client = await service.CreateAsync(new CreateClientDto { FullName = "A", DocumentNumber = "123", RefId = "R1" }, now);
        var membership = new Memberships
        {
            ClientId = client.ClientId,
            MembershipTypeId = membershipType.Id,
            StartDate = now.AddDays(-1),
            EndDate = now.AddDays(29),
            StatusId = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        await context.Memberships.AddAsync(membership);
        await context.SaveChangesAsync();

        var (items, _, _, _) = await service.ListAsync(null, null, 1, 10);
        var item = items.First(i => i.ClientId == client.ClientId);
        Assert.Equal(1, item.MembershipStatusId);
        Assert.Equal(membershipType.Id, item.MembershipTypeId);
    }
}
