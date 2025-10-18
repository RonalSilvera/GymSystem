using BusinessLogic.Ports;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IFactoryLogic _factory;

    public ReportsController(IFactoryLogic factory)
    {
        _factory = factory;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateReportModule(tenant);
        var result = await service.GetRevenueAsync(from, to);
        return Ok(new { daily = result.DailyTotals, total = result.GrandTotal });
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> Attendance([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 5)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateReportModule(tenant);
        var result = await service.GetAttendanceAsync(from, to, top);
        return Ok(new { byHour = result.ByHour, topClients = result.TopClients });
    }

    [HttpGet("expiring")]
    public async Task<IActionResult> Expiring([FromQuery] int days = 7)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateReportModule(tenant);
        var now = DateTime.UtcNow;
        var result = await service.GetExpiringAsync(now, days);
        return Ok(result);
    }

    [HttpGet("top-memberships")]
    public async Task<IActionResult> TopMemberships([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 5)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateReportModule(tenant);
        var result = await service.GetTopMembershipsAsync(from, to, top);
        return Ok(result);
    }
}
