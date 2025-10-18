using System;
using System.Threading.Tasks;
using BusinessLogic.Ports;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IFactoryLogic _factory;

    public DashboardController(IFactoryLogic factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateDashboardModule(tenant);
        var now = DateTime.UtcNow;
        var summary = await service.GetSummaryAsync(now);
        return Ok(summary);
    }
}
