using BusinessLogic.Ports;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace API.Controllers;

[ApiController]
[Route("api/access")]
public class AccessController : ControllerBase
{
    private readonly IFactoryLogic _factory;

    public AccessController(IFactoryLogic factory)
    {
        _factory = factory;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] AccessRequest request)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateAccessModule(tenant);
        var result = await service.ValidateAsync(request.Refid, request.DeviceId, DateTime.UtcNow);

        if (result.Allowed)
        {
            return Ok(new { allowed = true, clientId = result.ClientId, membershipId = result.MembershipId, message = result.Message });
        }

        return StatusCode(403, new { allowed = false, reason = result.Reason, message = result.Message });
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? clientId, [FromQuery] bool? allowed, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateAccessModule(tenant);
        var result = await service.ListLogsAsync(from, to, clientId, allowed, page, pageSize);
        return Ok(new { total = result.TotalCount, items = result.Items });
    }

    [HttpGet("logs/export")]
    public async Task<IActionResult> ExportLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? clientId, [FromQuery] bool? allowed)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateAccessModule(tenant);
        var logs = await service.ExportLogsAsync(from, to, clientId, allowed);
        var sb = new StringBuilder();
        sb.AppendLine("AccessTime,ClientId,ClientName,Allowed,DeviceId,Reason");
        foreach (var log in logs)
        {
            sb.AppendLine($"{log.AccessTime:o},{log.ClientId},\"{log.ClientName}\",{log.Allowed},{log.DeviceId},{log.Reason}");
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "access_logs.csv");
    }
}

public record AccessRequest(string Refid, string? DeviceId);
