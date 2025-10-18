using BusinessLogic.Ports;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Dto;
namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IFactoryLogic _factory;

        public RolesController(IFactoryLogic factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        /// <remarks>Returns every role defined in the system.</remarks>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, _, service) = _factory.CreateRoleModule(tenant);
        return Ok(await service.All());
        }

        /// <summary>
        /// Gets role details by id.
        /// </summary>
        /// <remarks>Returns 404 if the role is missing.</remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, _, service) = _factory.CreateRoleModule(tenant);
        return Ok(await service.Detail(id));
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <remarks>Used to manage access permissions.</remarks>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleDto role)
        {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, _, service) = _factory.CreateRoleModule(tenant);
        await service.Create(role);
        return Ok();
        }
    }
}
