using BusinessLogic.Ports;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserRolesController : ControllerBase
    {
        private readonly IFactoryLogic _factory;

        public UserRolesController(IFactoryLogic factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <remarks>Creates a relation between user and role.</remarks>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole(Guid userId, Guid roleId)
        {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, _, service) = _factory.CreateUserRoleModule(tenant);
        await service.Create(userId, roleId);
            return Ok();
        }

        /// <summary>
        /// Retrieves roles for a user.
        /// </summary>
        /// <remarks>Returns all roles associated to the user.</remarks>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetRoles(Guid userId)
        {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (_, _, service) = _factory.CreateUserRoleModule(tenant);
        var all = await service.All();
        return Ok(all.Where(r => r.UserId == userId));
        }
    }
}
