using Domain.Interface;
using Infrastructure.Interfaces;

namespace BusinessLogic.Ports;

public interface IFactoryLogic
{
    (IRoleRepository, IUnitOfWork, IRoleService) CreateRoleModule(string tenantId);
    (IUserRepository, IUnitOfWork, IUserService) CreateUserModule(string tenantId);
    (IUserRoleRepository, IUnitOfWork, IUserRoleService) CreateUserRoleModule(string tenantId);
    (IUserRepository, IUnitOfWork, IAuthService) CreateAuthModule(string tenantId);
    (IUnitOfWork, IAccessService) CreateAccessModule(string tenantId);
    (IUnitOfWork, IPaymentService) CreatePaymentModule(string tenantId);
    (IUnitOfWork, IMembershipService) CreateMembershipModule(string tenantId);
    (IUnitOfWork, IReportService) CreateReportModule(string tenantId);
    (IUnitOfWork, IDashboardService) CreateDashboardModule(string tenantId);
    (IUnitOfWork, IClientService) CreateClientModule(string tenantId);
    (IUnitOfWork, IMembershipTypeService) CreateMembershipTypeModule(string tenantId);
    (IUnitOfWork, IPaymentMethodService) CreatePaymentMethodModule(string tenantId);
    (IUnitOfWork, ICouponService) CreateCouponModule(string tenantId);
}
