using System;
using System.Reflection;
using Domain.Entity;
using Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public const string MigrationsHistoryTableName = "__EFMigrationsHistory_GSM";

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Users> Users { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Clients> Clients { get; set; }
    public DbSet<MembershipTypes> MembershipTypes { get; set; }
    public DbSet<Memberships> Memberships { get; set; }
    public DbSet<PaymentMethods> PaymentMethods { get; set; }
    public DbSet<Payments> Payments { get; set; }
    public DbSet<AccessLogs> AccessLogs { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Coupons> Coupons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            throw new InvalidOperationException("El DbContext debe ser configurado con una cadena de conexión en DbContextOptions.");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        SeedData(modelBuilder);
        modelBuilder.SeedBaseData();
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var superAdminRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var adminRoleId = Guid.Parse("44444444-4444-4444-4444-444444444445");
        var operatorRoleId = Guid.Parse("44444444-4444-4444-4444-444444444446");

        var superAdminId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var adminId = Guid.Parse("22222222-2222-2222-2222-222222222223");
        var operatorId = Guid.Parse("22222222-2222-2222-2222-222222222224");

        modelBuilder.Entity<Users>().HasData(
            new Users
            {
                UserId = superAdminId,
                Name = "GymSystem Super Admin",
                Email = "superadmin@org.com",
                Role = "SuperAdmin",
                PasswordHash = "$2b$11$UPc/LTgXfIZq/vzYo45PT.3COK5/KZNIJpllqrdXNwjpys3t4fS4y",
                ProfileImageUrl = null,
                StatusId = 1,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Users
            {
                UserId = adminId,
                Name = "GymSystem Administrator",
                Email = "admin@org.com",
                Role = "Admin",
                PasswordHash = "$2b$11$EbBlJrVeFOSF8gtmVPIBZOQVVdTYGbHFkUk9TV21l6wZuLOs6OZ2m",
                ProfileImageUrl = null,
                StatusId = 1,
                CreatedAt = now,
                UpdatedAt = now
            },
            new Users
            {
                UserId = operatorId,
                Name = "GymSystem Operator",
                Email = "operator@org.com",
                Role = "Operator",
                PasswordHash = "$2b$11$7rAE.8QI7EOeMOgUF4.6/OIN.R4AabZ66x37snH5NOf3il/KxTYLO",
                ProfileImageUrl = null,
                StatusId = 1,
                CreatedAt = now,
                UpdatedAt = now
            }
        );

        modelBuilder.Entity<UserRole>().HasData(
            new UserRole
            {
                UserRoleId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                UserId = superAdminId,
                RoleId = superAdminRoleId
            },
            new UserRole
            {
                UserRoleId = Guid.Parse("55555555-5555-5555-5555-555555555556"),
                UserId = adminId,
                RoleId = adminRoleId
            },
            new UserRole
            {
                UserRoleId = Guid.Parse("55555555-5555-5555-5555-555555555557"),
                UserId = operatorId,
                RoleId = operatorRoleId
            }
        );
    }
}
