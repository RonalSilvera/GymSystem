using System;
using Infrastructure.Dto;

namespace BusinessLogic.Ports;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(DateTime now);
}
