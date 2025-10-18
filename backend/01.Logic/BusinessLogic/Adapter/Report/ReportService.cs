using System.Linq;
using BusinessLogic.Ports;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Adapter.Report;

public class ReportService(AppDbContext context) : IReportService
{
    private readonly AppDbContext _context = context;

    private const int StatusActivo = 1;
    private const int StatusPagoParcial = 6;
    private const int StatusPagado = 7;
    private static readonly int[] ActiveMembershipStatuses = new[] { StatusActivo, StatusPagoParcial, StatusPagado };

    public async Task<RevenueReport> GetRevenueAsync(DateTime? from, DateTime? to)
    {
        var query = _context.Payments.Where(p => p.StatusId == StatusActivo);
        if (from.HasValue) query = query.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue) query = query.Where(p => p.PaymentDate <= to.Value);

        var payments = await query
            .Select(p => new { p.PaymentDate, Net = p.Amount - (p.DiscountAmount ?? 0) })
        .ToListAsync();

        var daily = payments
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new DailyRevenue(g.Key, g.Sum(x => x.Net)))
            .OrderBy(x => x.Date)
            .ToList();
        var total = daily.Sum(d => d.Total);
        return new RevenueReport(daily, total);
    }

    public async Task<AttendanceReport> GetAttendanceAsync(DateTime? from, DateTime? to, int topClients)
    {
        var query = _context.AccessLogs
            .Include(l => l.Client)
            .Where(l => l.StatusId == StatusActivo);
        if (from.HasValue) query = query.Where(l => l.AccessTime >= from.Value);
        if (to.HasValue) query = query.Where(l => l.AccessTime <= to.Value);

        var logs = await query.ToListAsync();

        var byHour = logs
            .GroupBy(l => new DateTime(l.AccessTime.Year, l.AccessTime.Month, l.AccessTime.Day, l.AccessTime.Hour, 0, 0))
            .Select(g => new HourlyAttendance(g.Key, g.Count()))
            .OrderBy(g => g.Hour)
            .ToList();

        var top = logs
            .GroupBy(l => new { l.ClientId, l.Client.FullName })
            .Select(g => new ClientAttendance(g.Key.ClientId, g.Key.FullName, g.Count()))
            .OrderByDescending(g => g.Count)
            .Take(topClients)
            .ToList();

        return new AttendanceReport(byHour, top);
    }

    public async Task<IReadOnlyList<ExpiringMembershipDto>> GetExpiringAsync(DateTime now, int days)
    {
        var until = now.AddDays(days);
        return await _context.Memberships
            .Include(m => m.Client)
            .Where(m => ActiveMembershipStatuses.Contains(m.StatusId) && m.EndDate >= now && m.EndDate <= until)
            .OrderBy(m => m.EndDate)
            .Select(m => new ExpiringMembershipDto(m.Id, m.ClientId, m.Client.FullName, m.EndDate))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TopMembershipDto>> GetTopMembershipsAsync(DateTime? from, DateTime? to, int topN)
    {
        var query = _context.Payments
            .Where(p => p.Amount > 0)
            .Join(_context.Memberships, p => p.MembershipId, m => m.Id, (p, m) => new { p, m.MembershipTypeId })
            .Join(_context.MembershipTypes, pm => pm.MembershipTypeId, mt => mt.Id, (pm, mt) => new { pm.p.PaymentDate, pm.MembershipTypeId, mt.Name });

        if (from.HasValue) query = query.Where(r => r.PaymentDate >= from.Value);
        if (to.HasValue) query = query.Where(r => r.PaymentDate <= to.Value);

        var records = await query.ToListAsync();

        return records
            .GroupBy(r => new { r.MembershipTypeId, r.Name })
            .Select(g => new TopMembershipDto(g.Key.MembershipTypeId, g.Key.Name, g.Count()))
            .OrderByDescending(t => t.Count)
            .Take(topN)
            .ToList();
    }
}
