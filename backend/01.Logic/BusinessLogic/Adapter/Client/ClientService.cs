using System.Net;
using System.Linq;
using BusinessLogic.Ports;
using Domain.Entity;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.Client;

public class ClientService(AppDbContext context, IUnitOfWork unitOfWork) : IClientService
{
    private readonly AppDbContext _context = context;
    private readonly IUnitOfWork _uow = unitOfWork;
    private const int StatusActivo = 1;
    private const int StatusInactivo = 2;
    private const int StatusPendiente = 5;

    public async Task<(IEnumerable<ClientDto> Items, int Total, int Active, int Inactive)> ListAsync(string? search, int? statusId, int page, int pageSize)
    {
        var query = _context.Clients.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c => c.FullName.ToLower().Contains(search) ||
                                     c.DocumentNumber.ToLower().Contains(search) ||
                                     (c.Email != null && c.Email.ToLower().Contains(search)) ||
                                     c.RefId.ToLower().Contains(search));
        }
        if (statusId.HasValue)
        {
            query = query.Where(c => c.StatusId == statusId.Value);
        }
        var total = await query.CountAsync();
        var active = await query.CountAsync(c => c.StatusId == StatusActivo);
        var inactive = await query.CountAsync(c => c.StatusId == StatusInactivo);
        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClientDto
            {
                ClientId = c.Id,
                FullName = c.FullName,
                DocumentNumber = c.DocumentNumber,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                RefId = c.RefId,
                StatusId = c.StatusId,
                MembershipTypeId = c.Memberships
                    .Where(m => m.StatusId != StatusInactivo)
                    .OrderByDescending(m => m.EndDate)
                    .Select(m => (int?)m.MembershipTypeId)
                    .FirstOrDefault(),
                MembershipTypeName = c.Memberships
                    .Where(m => m.StatusId != StatusInactivo)
                    .OrderByDescending(m => m.EndDate)
                    .Select(m => m.MembershipType.Name)
                    .FirstOrDefault(),
                MembershipStatusId = c.Memberships
                    .Where(m => m.StatusId != StatusInactivo)
                    .OrderByDescending(m => m.EndDate)
                    .Select(m => (int?)m.StatusId)
                    .FirstOrDefault(),
                MembershipStatusName = c.Memberships
                    .Where(m => m.StatusId != StatusInactivo)
                    .OrderByDescending(m => m.EndDate)
                    .Select(m => m.Status.Name)
                    .FirstOrDefault(),
                LastPaymentAmount = c.Memberships
                    .Where(m => m.StatusId != StatusInactivo)
                    .OrderByDescending(m => m.EndDate)
                    .Select(m => m.Payments
                        .Where(p => p.StatusId == StatusActivo)
                        .OrderByDescending(p => p.PaymentDate)
                        .Select(p => p.Amount - (p.DiscountAmount ?? 0m))
                        .FirstOrDefault())
                    .FirstOrDefault(),
                AmountPaid = c.Memberships
                    .Where(m => m.StatusId != StatusInactivo)
                    .OrderByDescending(m => m.EndDate)
                    .Select(m => m.Payments
                        .Where(p => p.StatusId == StatusActivo)
                        .Sum(p => p.Amount - (p.DiscountAmount ?? 0m)))
                    .FirstOrDefault(),
                AmountDue = c.Memberships
                    .Where(m => m.StatusId != StatusInactivo)
                    .OrderByDescending(m => m.EndDate)
                    .Select(m => m.MembershipType.Price - m.Payments
                        .Where(p => p.StatusId == StatusActivo)
                        .Sum(p => p.Amount - (p.DiscountAmount ?? 0m)))
                    .Select(balance => balance > 0 ? balance : 0m)
                    .FirstOrDefault()
            })
            .ToListAsync();
        return (items, total, active, inactive);
    }

    public async Task<ClientDto?> DetailAsync(int id)
    {
        var c = await _context.Clients
            .Include(x => x.Memberships)
                .ThenInclude(m => m.Status)
            .Include(x => x.Memberships)
                .ThenInclude(m => m.MembershipType)
            .Include(x => x.Memberships)
                .ThenInclude(m => m.Payments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return null;
        var membership = c.Memberships
            .Where(m => m.StatusId != StatusInactivo)
            .OrderByDescending(m => m.EndDate)
            .FirstOrDefault();
        return new ClientDto
        {
            ClientId = c.Id,
            FullName = c.FullName,
            DocumentNumber = c.DocumentNumber,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            RefId = c.RefId,
            StatusId = c.StatusId,
            MembershipTypeId = membership?.MembershipTypeId,
            MembershipTypeName = membership?.MembershipType.Name,
            MembershipStatusId = membership?.StatusId,
            MembershipStatusName = membership?.Status.Name,
            LastPaymentAmount = membership?.Payments
                .Where(p => p.StatusId == StatusActivo)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => p.Amount - (p.DiscountAmount ?? 0m))
                .FirstOrDefault(),
            AmountPaid = membership?.Payments
                .Where(p => p.StatusId == StatusActivo)
                .Sum(p => p.Amount - (p.DiscountAmount ?? 0m)),
            AmountDue = membership != null
                ? Math.Max(
                    membership.MembershipType.Price - membership.Payments
                        .Where(p => p.StatusId == StatusActivo)
                        .Sum(p => p.Amount - (p.DiscountAmount ?? 0m)),
                    0)
                : null
        };
    }

    public async Task<ClientDto> CreateAsync(CreateClientDto dto, DateTime now)
    {
        await EnsureUniqueAsync(dto.DocumentNumber, dto.RefId, dto.Email, null);
        var client = new Clients
        {
            FullName = dto.FullName,
            DocumentNumber = dto.DocumentNumber,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            RefId = dto.RefId,
            StatusId = StatusActivo,
            CreatedAt = now,
            UpdatedAt = now
        };
        _context.Clients.Add(client);
        await _uow.Complete();
        string? membershipStatusName = null;
        int? membershipStatusId = null;
        int? membershipTypeId = null;
        string? membershipTypeName = null;
        if (dto.MembershipTypeId.HasValue)
        {
            var mType = await _context.MembershipTypes.FirstAsync(t => t.Id == dto.MembershipTypeId.Value);
            var membership = new Memberships
            {
                ClientId = client.Id,
                MembershipTypeId = dto.MembershipTypeId.Value,
                StartDate = now,
                EndDate = now.AddDays(mType.DurationDays),
                StatusId = StatusPendiente,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.Memberships.AddAsync(membership);
            await _uow.Complete();
            membershipStatusName = "Pendiente de pago";
            membershipStatusId = StatusPendiente;
            membershipTypeId = dto.MembershipTypeId.Value;
            membershipTypeName = mType.Name;
        }
        return new ClientDto
        {
            ClientId = client.Id,
            FullName = client.FullName,
            DocumentNumber = client.DocumentNumber,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            RefId = client.RefId,
            StatusId = client.StatusId,
            MembershipTypeId = membershipTypeId,
            MembershipTypeName = membershipTypeName,
            MembershipStatusId = membershipStatusId,
            MembershipStatusName = membershipStatusName
        };
    }

    public async Task<ClientDto?> UpdateAsync(int id, UpdateClientDto dto, DateTime now)
    {
        var client = await _context.Clients
            .Include(c => c.Memberships)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (client == null) return null;
        await EnsureUniqueAsync(dto.DocumentNumber, dto.RefId, dto.Email, id);
        client.FullName = dto.FullName;
        client.DocumentNumber = dto.DocumentNumber;
        client.Email = dto.Email;
        client.Phone = dto.Phone;
        client.Address = dto.Address;
        client.RefId = dto.RefId;
        client.UpdatedAt = now;

        string? membershipStatusName = null;
        int? membershipStatusId = null;
        int? membershipTypeId = null;
        string? membershipTypeName = null;

        if (dto.ClearMembership)
        {
            foreach (var membership in client.Memberships.Where(m => m.StatusId != StatusInactivo))
            {
                membership.StatusId = StatusInactivo;
                if (membership.EndDate > now)
                {
                    membership.EndDate = now;
                }
                membership.UpdatedAt = now;
            }
        }
        else if (dto.MembershipTypeId.HasValue)
        {
            foreach (var membership in client.Memberships.Where(m => m.StatusId != StatusInactivo))
            {
                membership.StatusId = StatusInactivo;
                if (membership.EndDate > now)
                {
                    membership.EndDate = now;
                }
                membership.UpdatedAt = now;
            }
            var mType = await _context.MembershipTypes.FirstAsync(t => t.Id == dto.MembershipTypeId.Value);
            var newMembership = new Memberships
            {
                ClientId = client.Id,
                MembershipTypeId = dto.MembershipTypeId.Value,
                StartDate = now,
                EndDate = now.AddDays(mType.DurationDays),
                StatusId = StatusPendiente,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.Memberships.AddAsync(newMembership);
            membershipStatusName = "Pendiente de pago";
            membershipStatusId = StatusPendiente;
            membershipTypeId = dto.MembershipTypeId.Value;
            membershipTypeName = mType.Name;
        }

        await _uow.Complete();
        return new ClientDto
        {
            ClientId = client.Id,
            FullName = client.FullName,
            DocumentNumber = client.DocumentNumber,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address,
            RefId = client.RefId,
            StatusId = client.StatusId,
            MembershipTypeId = membershipTypeId,
            MembershipTypeName = membershipTypeName,
            MembershipStatusId = membershipStatusId,
            MembershipStatusName = membershipStatusName
        };
    }

    public async Task<bool> SoftDeleteAsync(int id, DateTime now)
    {
        var client = await _context.Clients
            .Include(c => c.Memberships)
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (client == null) return false;

        client.StatusId = StatusInactivo;
        client.UpdatedAt = now;

        foreach (var membership in client.Memberships)
        {
            membership.StatusId = StatusInactivo;
            if (membership.EndDate > now)
            {
                membership.EndDate = now;
            }
            membership.UpdatedAt = now;
        }

        foreach (var payment in client.Payments)
        {
            payment.StatusId = StatusInactivo;
            payment.UpdatedAt = now;
        }

        await _uow.Complete();
        return true;
    }

    public async Task<bool> ReactivateAsync(int id, DateTime now)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null) return false;

        client.StatusId = StatusActivo;
        client.UpdatedAt = now;
        await _uow.Complete();
        return true;
    }

    private async Task EnsureUniqueAsync(string documentNumber, string refId, string? email, int? excludeId)
    {
        if (await _context.Clients.AnyAsync(c => c.DocumentNumber == documentNumber && c.Id != excludeId))
        {
            throw new BusinessException(HttpStatusCode.Conflict, "Document number already exists", "Document number already exists");
        }
        if (await _context.Clients.AnyAsync(c => c.RefId == refId && c.Id != excludeId))
        {
            throw new BusinessException(HttpStatusCode.Conflict, "RefId already exists", "RefId already exists");
        }
        if (!string.IsNullOrWhiteSpace(email) &&
            await _context.Clients.AnyAsync(c => c.Email == email && c.Id != excludeId))
        {
            throw new BusinessException(HttpStatusCode.Conflict, "Email already exists", "Email already exists");
        }
    }
}
