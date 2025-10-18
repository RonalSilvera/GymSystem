using BusinessLogic.Ports;
using Infrastructure.Dto;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipTypesController : ControllerBase
{
    private readonly IFactoryLogic _factory;
    public MembershipTypesController(IFactoryLogic factory)
    {
        _factory = factory;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateMembershipTypeModule(tenant);
        var items = await service.ListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateMembershipTypeModule(tenant);
        var item = await service.DetailAsync(id);
        return item != null ? Ok(item) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateMembershipTypeDto dto)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateMembershipTypeModule(tenant);
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] UpdateMembershipTypeDto dto)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateMembershipTypeModule(tenant);
        var updated = await service.UpdateAsync(id, dto);
        return updated != null ? Ok(updated) : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, service) = _factory.CreateMembershipTypeModule(tenant);
        var removed = await service.DeleteAsync(id);
        return removed ? NoContent() : NotFound();
    }
}
