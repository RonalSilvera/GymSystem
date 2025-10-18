using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using BusinessLogic.Ports;
using Infrastructure.Data;
using Infrastructure.Dto;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.Dashboard;

public class DashboardService(AppDbContext context) : IDashboardService
{
    private readonly AppDbContext _context = context;

    private const int StatusActivo = 1;
    private const int StatusPagoParcial = 6;
    private const int StatusPagado = 7;
    private static readonly int[] ActiveMembershipStatuses = new[] { StatusActivo, StatusPagoParcial, StatusPagado };

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime now)
    {
        var today = now.Date;
        var startOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
        var startPeriod = startOfCurrentMonth.AddMonths(-5);
        var endPeriod = startOfCurrentMonth.AddMonths(1);
        var expirationLimit = now.AddDays(7);

        var clientsCounters = await _context.Clients.AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(c => c.StatusId == StatusActivo)
            })
            .SingleOrDefaultAsync();
        var totalClients = clientsCounters?.Total ?? 0;
        var activeClients = clientsCounters?.Active ?? 0;
        var expiringMemberships = await _context.Memberships.AsNoTracking()
            .Where(m => ActiveMembershipStatuses.Contains(m.StatusId) && m.EndDate >= now && m.EndDate <= expirationLimit)
            .CountAsync();
        var todaysAccesses = await _context.AccessLogs.AsNoTracking()
            .Where(a => a.StatusId == StatusActivo && a.AccessTime >= today && a.AccessTime < today.AddDays(1))
            .CountAsync();

        var monthlyClientsRaw = await _context.Clients.AsNoTracking()
            .Where(c => c.CreatedAt >= startPeriod && c.CreatedAt < endPeriod)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync();

        var membershipDistribution = await _context.Payments.AsNoTracking()
            .Where(p => p.StatusId == StatusActivo && p.PaymentDate >= startPeriod && p.PaymentDate < endPeriod)
            .Join(_context.Memberships, p => p.MembershipId, m => m.Id, (p, m) => new { m.MembershipTypeId })
            .Join(_context.MembershipTypes, pm => pm.MembershipTypeId, mt => mt.Id, (pm, mt) => new { pm.MembershipTypeId, mt.Name })
            .GroupBy(x => new { x.MembershipTypeId, x.Name })
            .Select(g => new MembershipTypeDistributionDto
            {
                MembershipTypeId = g.Key.MembershipTypeId,
                Name = g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var monthlyDataRaw = monthlyClientsRaw.ToDictionary(
            x => (x.Year, x.Month),
            x => x.Count);
        var culture = new CultureInfo("es-ES");
        var textInfo = culture.TextInfo;
        var monthlyData = new List<MonthlyCountDto>(capacity: 6);
        for (var offset = 5; offset >= 0; offset--)
        {
            var monthDate = startOfCurrentMonth.AddMonths(-offset);
            monthlyData.Add(new MonthlyCountDto
            {
                Year = monthDate.Year,
                Month = monthDate.Month,
                Label = textInfo.ToTitleCase(monthDate.ToString("MMMM", culture)),
                Count = monthlyDataRaw.TryGetValue((monthDate.Year, monthDate.Month), out var count) ? count : 0
            });
        }

        return new DashboardSummaryDto
        {
            TotalClients = totalClients,
            ActiveClients = activeClients,
            ExpiringMemberships = expiringMemberships,
            TodaysAccesses = todaysAccesses,
            MonthlyNewClients = monthlyData,
            MembershipTypeDistribution = membershipDistribution
        };
    }
}
