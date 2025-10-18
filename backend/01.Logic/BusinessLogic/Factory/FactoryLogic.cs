using AutoMapper;
using BusinessLogic.Adapter.Access;
using BusinessLogic.Adapter.Auth;
using BusinessLogic.Adapter.Client;
using BusinessLogic.Adapter.Dashboard;
using BusinessLogic.Adapter.Coupon;
using BusinessLogic.Adapter.FileStorage;
using BusinessLogic.Adapter.Membership;
using BusinessLogic.Adapter.MembershipType;
using BusinessLogic.Adapter.Payment;
using BusinessLogic.Adapter.PaymentMethod;
using BusinessLogic.Adapter.Report;
using BusinessLogic.Adapter.Role;
using BusinessLogic.Adapter.User;
using BusinessLogic.Adapter.UserRole;
using BusinessLogic.Ports;
using Domain.Interface;
using Infrastructure.Interfaces;
using Infrastructure.WorkUnit;
using Microsoft.Extensions.Configuration;
using Repository.Role;
using Repository.User;
using Repository.UserRole;

public class FactoryLogic(IAppDbContextFactory contextFactory, IMapper mapper, IConfiguration config) : IFactoryLogic
{
    public (IUserRepository, IUnitOfWork, IUserService) CreateUserModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var repo = new UserRepository(context);
        var fileStorage = new LocalFileStorage();
        var service = new UserService(repo, unitOfWork, mapper, fileStorage);
        return (repo, unitOfWork, service);
    }

    public (IRoleRepository, IUnitOfWork, IRoleService) CreateRoleModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var repo = new RoleRepository(context);
        var service = new RoleService(repo, unitOfWork, mapper);
        return (repo, unitOfWork, service);
    }

    public (IUserRoleRepository, IUnitOfWork, IUserRoleService) CreateUserRoleModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var repo = new UserRoleRepository(context);
        var service = new UserRoleService(repo, unitOfWork);
        return (repo, unitOfWork, service);
    }

    public (IUserRepository, IUnitOfWork, IAuthService) CreateAuthModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var repo = new UserRepository(context);
        var service = new AuthService(repo, unitOfWork, config);
        return (repo, unitOfWork, service);
    }

    public (IUnitOfWork, IAccessService) CreateAccessModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new AccessService(context, unitOfWork);
        return (unitOfWork, service);
    }

    public (IUnitOfWork, IPaymentService) CreatePaymentModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new PaymentService(context, unitOfWork);
        return (unitOfWork, service);
    }

    public (IUnitOfWork, IMembershipService) CreateMembershipModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new MembershipService(context, unitOfWork);
        return (unitOfWork, service);
    }

    public (IUnitOfWork, IClientService) CreateClientModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new ClientService(context, unitOfWork);
        return (unitOfWork, service);
    }

    public (IUnitOfWork, IMembershipTypeService) CreateMembershipTypeModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new MembershipTypeService(context, unitOfWork);
        return (unitOfWork, service);
    }

    public (IUnitOfWork, IPaymentMethodService) CreatePaymentMethodModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new PaymentMethodService(context, unitOfWork);
        return (unitOfWork, service);
    }

    public (IUnitOfWork, ICouponService) CreateCouponModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new CouponService(context, unitOfWork);
        return (unitOfWork, service);
    }

    public (IUnitOfWork, IReportService) CreateReportModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new ReportService(context);
        return (unitOfWork, service);
    }

    public (IUnitOfWork, IDashboardService) CreateDashboardModule(string tenantId)
    {
        var context = contextFactory.CreateDbContext(tenantId);
        var unitOfWork = new UnitOfWork(context);
        var service = new DashboardService(context);
        return (unitOfWork, service);
    }
}
