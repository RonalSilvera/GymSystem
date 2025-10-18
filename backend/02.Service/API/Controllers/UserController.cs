using BusinessLogic.Ports;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Dto;
using System.Security.Claims;
using Domain.Common.Enums;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IFactoryLogic _factory;

        public UsersController(IFactoryLogic factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <remarks>Returns the complete list of registered users.</remarks>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
            var (_, _, service) = _factory.CreateUserModule(tenant);
            return Ok(await service.All());
        }

        /// <summary>
        /// Deletes a user by identifier.
        /// </summary>
        /// <remarks>Removes the user from the system when it exists.</remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role == RoleNames.Operator)
            {
                return Forbid();
            }

            var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
            var (_, _, service) = _factory.CreateUserModule(tenant);
            var target = await service.Detail(id);
            if (target == null)
            {
                return NotFound();
            }

            if (role == RoleNames.Admin &&
                (target.Role == RoleNames.Admin || target.Role == RoleNames.SuperAdmin))
            {
                return Forbid();
            }

            await service.Delete(id);
            return NoContent();
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <remarks>Creates the user account.</remarks>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto user)
        {
            var currentRole = (User.FindFirstValue(ClaimTypes.Role) ?? string.Empty).ToUpperInvariant();
            var newRole = (user.Role ?? string.Empty).ToUpperInvariant();

            var allowedRoles = currentRole switch
            {
                var r when r == RoleNames.SuperAdmin.ToUpperInvariant() => new[]
                {
                    RoleNames.Admin.ToUpperInvariant(),
                    RoleNames.Operator.ToUpperInvariant()
                },
                var r when r == RoleNames.Admin.ToUpperInvariant() => new[]
                {
                    RoleNames.Operator.ToUpperInvariant()
                },
                _ => Array.Empty<string>()
            };

            if (!allowedRoles.Contains(newRole))
            {
                return Forbid();
            }

            var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
            var (_, _, service) = _factory.CreateUserModule(tenant);
            await service.Create(user);
            return Ok();
        }

        /// <summary>
        /// Retrieves a user by identifier.
        /// </summary>
        /// <remarks>Returns 404 when the user is not found.</remarks>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
            var (_, _, service) = _factory.CreateUserModule(tenant);
            var user = await service.Detail(id);
            return user != null ? Ok(user) : NotFound();
        }

        /// <summary>
        /// Updates a user and profile image.
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <param name="payload">Data to update the user including an optional base64 image</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto payload)
        {
            var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
            var (_, _, service) = _factory.CreateUserModule(tenant);
            var updated = await service.Update(id, payload);
            return updated != null ? Ok(updated) : NotFound();
        }
    }
}
