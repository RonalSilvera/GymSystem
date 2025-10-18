using System.Collections.Generic;

namespace Infrastructure.Dto;

public class DashboardSummaryDto
{
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int ExpiringMemberships { get; set; }
    public int TodaysAccesses { get; set; }
    public IReadOnlyList<MonthlyCountDto> MonthlyNewClients { get; set; } = new List<MonthlyCountDto>();
    public IReadOnlyList<MembershipTypeDistributionDto> MembershipTypeDistribution { get; set; } = new List<MembershipTypeDistributionDto>();
}
