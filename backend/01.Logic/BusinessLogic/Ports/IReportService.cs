using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BusinessLogic.Ports;

public record DailyRevenue(DateTime Date, decimal Total);
public record RevenueReport(IReadOnlyList<DailyRevenue> DailyTotals, decimal GrandTotal);

public record HourlyAttendance(DateTime Hour, int Count);
public record ClientAttendance(int ClientId, string ClientName, int Count);
public record AttendanceReport(IReadOnlyList<HourlyAttendance> ByHour, IReadOnlyList<ClientAttendance> TopClients);

public record ExpiringMembershipDto(int MembershipId, int ClientId, string ClientName, DateTime EndDate);
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);

public record TopMembershipDto(int MembershipTypeId, string Name, int Count);

public interface IReportService
{
    Task<RevenueReport> GetRevenueAsync(DateTime? from, DateTime? to);
    Task<AttendanceReport> GetAttendanceAsync(DateTime? from, DateTime? to, int topClients);
    Task<IReadOnlyList<ExpiringMembershipDto>> GetExpiringAsync(DateTime now, int days);
    Task<IReadOnlyList<TopMembershipDto>> GetTopMembershipsAsync(DateTime? from, DateTime? to, int topN);
}
