using BusinessLogic.Adapter.MembershipType;
using Domain.Entity;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.WorkUnit;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Xunit;

public class MembershipTypeServiceTests
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
    public async Task Create_AddsMembershipType()
    {
        var context = CreateContext(nameof(Create_AddsMembershipType));
        var service = new MembershipTypeService(context, new UnitOfWork(context));
        var dto = new CreateMembershipTypeDto { Name = "Test", DurationDays = 10, Price = 100m };
        var created = await service.CreateAsync(dto);
        Assert.Equal("Test", created.Name);
        var saved = await context.MembershipTypes.FindAsync(created.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Create_DuplicateName_Throws()
    {
        var context = CreateContext(nameof(Create_DuplicateName_Throws));
        var service = new MembershipTypeService(context, new UnitOfWork(context));
        await service.CreateAsync(new CreateMembershipTypeDto { Name = "A", DurationDays = 1, Price = 1m });
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateAsync(new CreateMembershipTypeDto { Name = "A", DurationDays = 2, Price = 2m }));
        Assert.Equal(HttpStatusCode.Conflict, ex.Status);
    }

    [Fact]
    public async Task Delete_WithMemberships_SoftDeletes()
    {
        var context = CreateContext(nameof(Delete_WithMemberships_SoftDeletes));
        var service = new MembershipTypeService(context, new UnitOfWork(context));
        var mt = await service.CreateAsync(new CreateMembershipTypeDto { Name = "A", DurationDays = 1, Price = 1m });
        var now = DateTime.UtcNow;
        var client = new Clients { FullName = "C", DocumentNumber = "1", RefId = "R1", StatusId = 1, CreatedAt = now, UpdatedAt = now };
        context.Clients.Add(client);
        await context.SaveChangesAsync();
        var membership = new Memberships { ClientId = client.Id, MembershipTypeId = mt.Id, StartDate = now, EndDate = now, StatusId = 1, CreatedAt = now, UpdatedAt = now };
        context.Memberships.Add(membership);
        await context.SaveChangesAsync();
        var result = await service.DeleteAsync(mt.Id);
        Assert.True(result);
        var entity = await context.MembershipTypes.FindAsync(mt.Id);
        Assert.False(entity!.Active);
    }

    [Fact]
    public async Task Delete_WithoutReferences_Removes()
    {
        var context = CreateContext(nameof(Delete_WithoutReferences_Removes));
        var service = new MembershipTypeService(context, new UnitOfWork(context));
        var mt = await service.CreateAsync(new CreateMembershipTypeDto { Name = "A", DurationDays = 1, Price = 1m });
        var result = await service.DeleteAsync(mt.Id);
        Assert.True(result);
        var entity = await context.MembershipTypes.FindAsync(mt.Id);
        Assert.Null(entity);
    }
}
