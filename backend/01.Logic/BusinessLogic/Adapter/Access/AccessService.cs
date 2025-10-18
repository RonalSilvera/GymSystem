using System.Linq;
using BusinessLogic.Ports;
using Domain.Entity;
using Infrastructure.Data;
using Infrastructure.Dto;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.Access;

public class AccessService(AppDbContext context, IUnitOfWork unitOfWork) : IAccessService
{
    private readonly AppDbContext _context = context;
    private readonly IUnitOfWork _uow = unitOfWork;

    private const int StatusActivo = 1;
    private const int StatusInactivo = 2;
    private const int StatusPagoParcial = 6;
    private const int StatusPagado = 7;
    private static readonly int[] ActiveMembershipStatuses = new[] { StatusActivo, StatusPagoParcial, StatusPagado };

    public async Task<AccessValidationResult> ValidateAsync(string refid, string? deviceId, DateTime now)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.RefId == refid);
        if (client == null || client.StatusId != StatusActivo)
        {
            if (client != null)
            {
                await LogAsync(client.Id, now, StatusInactivo, deviceId, "inactive");
                await _uow.Complete();
            }
            return new AccessValidationResult(false, client?.Id, null, "Cliente no activo", "inactive");
        }

        var membership = await _context.Memberships
            .FirstOrDefaultAsync(m => m.ClientId == client.Id &&
                                      ActiveMembershipStatuses.Contains(m.StatusId) &&
                                      now >= m.StartDate &&
                                      now <= m.EndDate);
        if (membership == null)
        {
            await LogAsync(client.Id, now, StatusInactivo, deviceId, "vencido");
            await _uow.Complete();
            return new AccessValidationResult(false, client.Id, null, "Membresía vencida", "vencido");
        }

        var lastAllowed = await _context.AccessLogs
            .Where(l => l.ClientId == client.Id && l.StatusId == StatusActivo)
            .OrderByDescending(l => l.AccessTime)
            .FirstOrDefaultAsync();

        if (lastAllowed != null && (now - lastAllowed.AccessTime) < TimeSpan.FromMinutes(5))
        {
            await LogAsync(client.Id, now, StatusInactivo, deviceId, "cooldown");
            await _uow.Complete();
            return new AccessValidationResult(false, client.Id, membership.Id, "Acceso en cooldown", "cooldown");
        }

        await LogAsync(client.Id, now, StatusActivo, deviceId, null);
        await _uow.Complete();
        return new AccessValidationResult(true, client.Id, membership.Id, "Acceso permitido");
    }

    public async Task<PagedResult<AccessLogDto>> ListLogsAsync(DateTime? from, DateTime? to, int? clientId, bool? allowed, int page, int pageSize)
    {
        var query = _context.AccessLogs.Include(l => l.Client).AsQueryable();
        if (from.HasValue)
            query = query.Where(l => l.AccessTime >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.AccessTime <= to.Value);
        if (clientId.HasValue)
            query = query.Where(l => l.ClientId == clientId.Value);
        if (allowed.HasValue)
            query = allowed.Value ? query.Where(l => l.StatusId == StatusActivo) : query.Where(l => l.StatusId == StatusInactivo);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.AccessTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AccessLogDto
            {
                Id = l.Id,
                ClientId = l.ClientId,
                ClientName = l.Client.FullName,
                AccessTime = l.AccessTime,
                Allowed = l.StatusId == StatusActivo,
                DeviceId = l.DeviceId,
                Reason = l.Reason
            }).ToListAsync();
        return new PagedResult<AccessLogDto>(items, total);
    }

    public async Task<IReadOnlyList<AccessLogDto>> ExportLogsAsync(DateTime? from, DateTime? to, int? clientId, bool? allowed)
    {
        var query = _context.AccessLogs.Include(l => l.Client).AsQueryable();
        if (from.HasValue)
            query = query.Where(l => l.AccessTime >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.AccessTime <= to.Value);
        if (clientId.HasValue)
            query = query.Where(l => l.ClientId == clientId.Value);
        if (allowed.HasValue)
            query = allowed.Value ? query.Where(l => l.StatusId == StatusActivo) : query.Where(l => l.StatusId == StatusInactivo);

        return await query.OrderByDescending(l => l.AccessTime)
            .Select(l => new AccessLogDto
            {
                Id = l.Id,
                ClientId = l.ClientId,
                ClientName = l.Client.FullName,
                AccessTime = l.AccessTime,
                Allowed = l.StatusId == StatusActivo,
                DeviceId = l.DeviceId,
                Reason = l.Reason
            }).ToListAsync();
    }

    private async Task LogAsync(int clientId, DateTime now, int statusId, string? deviceId, string? reason)
    {
        var log = new AccessLogs
        {
            ClientId = clientId,
            AccessTime = now,
            StatusId = statusId,
            DeviceId = deviceId,
            Reason = reason
        };
        await _context.AccessLogs.AddAsync(log);
    }
}
