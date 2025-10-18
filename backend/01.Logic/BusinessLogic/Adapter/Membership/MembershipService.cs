using BusinessLogic.Ports;
using Domain.Entity;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.Membership;

public class MembershipService(AppDbContext context, IUnitOfWork unitOfWork) : IMembershipService
{
    private readonly AppDbContext _context = context;
    private readonly IUnitOfWork _uow = unitOfWork;
    private const int StatusActivo = 1;
    private const int StatusInactivo = 2;
    private const int StatusVencido = 3;
    private const int StatusPagoParcial = 6;
    private const int StatusPagado = 7;
    private static bool IsActiveStatus(int statusId) =>
        statusId == StatusActivo || statusId == StatusPagoParcial || statusId == StatusPagado;

    public async Task<MembershipActivationResult> ActivateOrRenewAsync(int clientId, int membershipTypeId, DateTime? startDate, DateTime now)
    {
        var mType = await _context.MembershipTypes.FirstAsync(t => t.Id == membershipTypeId);

        var memberships = await _context.Memberships
            .Where(m => m.ClientId == clientId)
            .ToListAsync();

        foreach (var m in memberships)
        {
            if (now < m.StartDate && m.StatusId != StatusInactivo)
            {
                m.StatusId = StatusInactivo;
                m.UpdatedAt = now;
            }
            else if (now > m.EndDate && m.StatusId != StatusVencido)
            {
                m.StatusId = StatusVencido;
                m.UpdatedAt = now;
            }
            else if (now >= m.StartDate && now <= m.EndDate && !IsActiveStatus(m.StatusId))
            {
                m.StatusId = StatusActivo;
                m.UpdatedAt = now;
            }
        }

        var active = memberships.FirstOrDefault(m => IsActiveStatus(m.StatusId) && now >= m.StartDate && now <= m.EndDate);

        if (active != null)
        {
            if (active.MembershipTypeId == membershipTypeId)
            {
                await _uow.Complete();
                return new MembershipActivationResult(active.Id, active.StartDate, active.EndDate, "Activo");
            }

            active.EndDate = now.AddSeconds(-1);
            active.StatusId = StatusInactivo;
            active.UpdatedAt = now;

            var newMembership = new Memberships
            {
                ClientId = clientId,
                MembershipTypeId = membershipTypeId,
                StartDate = now,
                EndDate = now.AddDays(mType.DurationDays),
                StatusId = StatusActivo,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.Memberships.AddAsync(newMembership);
            await _uow.Complete();
            return new MembershipActivationResult(newMembership.Id, newMembership.StartDate, newMembership.EndDate, "Activo");
        }

        var start = startDate ?? now;
        var statusId = now >= start && now <= start.AddDays(mType.DurationDays) ? StatusActivo : StatusInactivo;
        var membership = new Memberships
        {
            ClientId = clientId,
            MembershipTypeId = membershipTypeId,
            StartDate = start,
            EndDate = start.AddDays(mType.DurationDays),
            StatusId = statusId,
            CreatedAt = now,
            UpdatedAt = now
        };
        await _context.Memberships.AddAsync(membership);
        await _uow.Complete();

        var statusName = statusId == StatusActivo ? "Activo" : "Inactivo";
        return new MembershipActivationResult(membership.Id, membership.StartDate, membership.EndDate, statusName);
    }
}
