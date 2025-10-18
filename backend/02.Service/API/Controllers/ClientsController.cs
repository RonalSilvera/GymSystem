using BusinessLogic.Ports;
using Infrastructure.Dto;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IFactoryLogic _factory;
    public ClientsController(IFactoryLogic factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int? statusId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateClientModule(tenant);
        var (items, total, active, inactive) = await service.ListAsync(search, statusId, page, pageSize);
        return Ok(new { total, active, inactive, items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateClientModule(tenant);
        var client = await service.DetailAsync(id);
        return client != null ? Ok(client) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateClientDto dto)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateClientModule(tenant);
        var now = DateTime.UtcNow;
        var created = await service.CreateAsync(dto, now);
        return CreatedAtAction(nameof(GetById), new { id = created.ClientId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateClientDto dto)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateClientModule(tenant);
        var now = DateTime.UtcNow;
        var updated = await service.UpdateAsync(id, dto, now);
        return updated != null ? Ok(updated) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateClientModule(tenant);
        var now = DateTime.UtcNow;
        var removed = await service.SoftDeleteAsync(id, now);
        return removed ? NoContent() : NotFound();
    }

    [HttpPatch("{id}/reactivate")]
    public async Task<IActionResult> Reactivate(int id)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateClientModule(tenant);
        var now = DateTime.UtcNow;
        var activated = await service.ReactivateAsync(id, now);
        return activated ? NoContent() : NotFound();
    }
}
