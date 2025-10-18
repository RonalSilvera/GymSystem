using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Domain.Entity;
using Xunit;

public class EntitiesTests
{
    public static IEnumerable<object[]> Entities =>
        typeof(Users).Assembly.GetTypes()
            .Where(t => t.Namespace == "Domain.Entity" && t.IsClass)
            .Select(t => new object[] { t });

    [Theory]
    [MemberData(nameof(Entities))]
    public void Entity_IdPropertyRoundtrip(Type type)
    {
        var instance = Activator.CreateInstance(type)!;
        var prop = type.GetProperties().FirstOrDefault(p => p.PropertyType == typeof(Guid) && p.Name.EndsWith("Id"));
        if (prop == null) return; // skip when no id
        var id = Guid.NewGuid();
        prop.SetValue(instance, id);
        Assert.Equal(id, prop.GetValue(instance));
    }

    [Theory]
    [MemberData(nameof(Entities))]
    public void Entity_CollectionsInitialized(Type type)
    {
        var instance = Activator.CreateInstance(type)!;
        foreach(var p in type.GetProperties().Where(p => typeof(System.Collections.IEnumerable).IsAssignableFrom(p.PropertyType) && p.PropertyType != typeof(string)))
        {
            Assert.NotNull(p.GetValue(instance));
        }
    }
}
