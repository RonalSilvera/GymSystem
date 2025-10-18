using System;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Extensions;

public static class ModelBuilderExtensions
{
    public static void SeedBaseData(this ModelBuilder modelBuilder)
    {
        // Roles base
        var superAdminRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var adminRoleId = Guid.Parse("44444444-4444-4444-4444-444444444445");
        var operatorRoleId = Guid.Parse("44444444-4444-4444-4444-444444444446");

        modelBuilder.Entity<Roles>().HasData(
            new Roles { RoleId = superAdminRoleId, Name = "SuperAdmin", Description = "Super administrator role" },
            new Roles { RoleId = adminRoleId, Name = "Admin", Description = "Administrator role" },
            new Roles { RoleId = operatorRoleId, Name = "Operator", Description = "Operator role" }
        );

        // Status
        modelBuilder.Entity<Status>().HasData(
            new Status { Id = 1, Name = "Activo" },
            new Status { Id = 2, Name = "Inactivo" },
            new Status { Id = 3, Name = "Vencido" },
            new Status { Id = 4, Name = "Suspendido" },
            new Status { Id = 5, Name = "Pendiente de pago" }
        );

        // Membership types
        modelBuilder.Entity<MembershipTypes>().HasData(
            new MembershipTypes { Id = 1, Name = "Diaria", Description = "Acceso por 1 día", DurationDays = 1, Price = 10000m, Active = true },
            new MembershipTypes { Id = 2, Name = "Mensual", Description = "Acceso por 30 días", DurationDays = 30, Price = 60000m, Active = true },
            new MembershipTypes { Id = 3, Name = "Anual", Description = "Acceso por 365 días", DurationDays = 365, Price = 600000m, Active = true }
        );

        // Payment methods
        modelBuilder.Entity<PaymentMethods>().HasData(
            new PaymentMethods { Id = 1, Name = "Efectivo", Active = true },
            new PaymentMethods { Id = 2, Name = "Tarjeta", Active = true },
            new PaymentMethods { Id = 3, Name = "Nequi", Active = true },
            new PaymentMethods { Id = 4, Name = "Bancolombia", Active = true }
        );
    }
}

