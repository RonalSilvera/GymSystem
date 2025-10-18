using System.Security.Claims;
using API.Models.Auth;
using API.Services;
using BusinessLogic.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IFactoryLogic _factory;
    private readonly IJwtTokenService _tokenService;
    private readonly IPasswordService _passwordService;

    public AuthController(IFactoryLogic factory, IJwtTokenService tokenService, IPasswordService passwordService)
    {
        _factory = factory;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var tenant = HttpContext.Items["Tenant"]?.ToString() ?? string.Empty;
        var (repo, _, _) = _factory.CreateAuthModule(tenant);
        var user = await repo.FindByEmail(request.Email);
        const int StatusActivo = 1;
        if (user == null || user.StatusId != StatusActivo || !_passwordService.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var (token, expiresAtUtc) = _tokenService.GenerateToken(user);
        var response = new LoginResponse(token, expiresAtUtc, user.Role, user.Email);
        return Ok(response);
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(new { userId, email, role, claims });
    }
}
