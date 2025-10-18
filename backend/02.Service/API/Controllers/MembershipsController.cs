using BusinessLogic.Ports;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/memberships")]
public class MembershipsController : ControllerBase
{
    private readonly IFactoryLogic _factory;

    public MembershipsController(IFactoryLogic factory)
    {
        _factory = factory;
    }

    [HttpPost("activate-or-renew")]
    public async Task<IActionResult> ActivateOrRenew([FromBody] ActivateOrRenewRequest request)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateMembershipModule(tenant);
        var now = DateTime.UtcNow;
        var result = await service.ActivateOrRenewAsync(request.ClientId, request.MembershipTypeId, request.StartDate, now);
        return Ok(new { membershipId = result.MembershipId, startDate = result.StartDate, endDate = result.EndDate, status = result.Status });
    }
}

public record ActivateOrRenewRequest(int ClientId, int MembershipTypeId, DateTime? StartDate);
