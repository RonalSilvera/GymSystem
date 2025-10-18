using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Repository.Role;
using Repository.User;
using Repository.UserRole;
using Xunit;

public class RepositoryTests
{
    private static DbContextOptions<AppDbContext> Options => new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    public static IEnumerable<object[]> Repos => new object[][]
    {
        new object[] { typeof(RoleRepository), typeof(Domain.Entity.Roles), "RoleId" },
        new object[] { typeof(UserRepository), typeof(Domain.Entity.Users), "UserId" },
        new object[] { typeof(UserRoleRepository), typeof(Domain.Entity.UserRole), "UserRoleId" },
    };

    [Theory]
    [MemberData(nameof(Repos))]
    public async Task Create_And_Detail_Success(Type repoType, Type entityType, string key)
    {
        var options = Options;
        using var context = new AppDbContext(options);
        dynamic repo;
        var ctor = repoType.GetConstructors().First();
        var parameters = ctor.GetParameters();
        if (parameters.Length == 2)
        {
            var uowMock = new Moq.Mock<Infrastructure.Interfaces.IUnitOfWork>();
            repo = Activator.CreateInstance(repoType, context, uowMock.Object)!;
        }
        else
        {
            repo = Activator.CreateInstance(repoType, context)!;
        }
        dynamic entity = Activator.CreateInstance(entityType)!;
        var id = Guid.NewGuid();
        entityType.GetProperty(key)!.SetValue(entity, id);
        foreach (var prop in entityType.GetProperties().Where(p => p.PropertyType == typeof(string)))
        {
            prop.SetValue(entity, "test");
        }
        await repo.Create(entity);
        await context.SaveChangesAsync();
        var found = await repo.Detail(id);
        Assert.NotNull(found);
    }

    [Theory]
    [MemberData(nameof(Repos))]
    public async Task Detail_ReturnsNull_WhenMissing(Type repoType, Type entityType, string key)
    {
        var options = Options;
        using var context = new AppDbContext(options);
        dynamic repo;
        var ctor = repoType.GetConstructors().First();
        var parameters = ctor.GetParameters();
        if (parameters.Length == 2)
        {
            var uowMock = new Moq.Mock<Infrastructure.Interfaces.IUnitOfWork>();
            repo = Activator.CreateInstance(repoType, context, uowMock.Object)!;
        }
        else
        {
            repo = Activator.CreateInstance(repoType, context)!;
        }
        var missing = await repo.Detail(Guid.NewGuid());
        Assert.Null(missing);
    }

    [Theory]
    [MemberData(nameof(Repos))]
    public async Task All_Update_Delete_Work(Type repoType, Type entityType, string key)
    {
        var options = Options;
        using var context = new AppDbContext(options);
        dynamic repo;
        var ctor = repoType.GetConstructors().First();
        var parameters = ctor.GetParameters();
        if (parameters.Length == 2)
        {
            var uowMock = new Moq.Mock<Infrastructure.Interfaces.IUnitOfWork>();
            repo = Activator.CreateInstance(repoType, context, uowMock.Object)!;
        }
        else
        {
            repo = Activator.CreateInstance(repoType, context)!;
        }
        dynamic entity = Activator.CreateInstance(entityType)!;
        var id = Guid.NewGuid();
        entityType.GetProperty(key)!.SetValue(entity, id);
        foreach (var prop in entityType.GetProperties().Where(p => p.PropertyType == typeof(string)))
        {
            prop.SetValue(entity, "test");
        }
        await repo.Create(entity);
        await context.SaveChangesAsync();
        await repo.Update(entity);
        await repo.Delete(entity);
        await context.SaveChangesAsync();
        var list = await repo.All();
        Assert.Empty(list);
    }
}
