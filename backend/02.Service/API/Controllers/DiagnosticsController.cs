using System.IdentityModel.Tokens.Jwt;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/_diag")]
public class DiagnosticsController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;

    public DiagnosticsController(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService;
    }

    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping() => Ok("pong");

    [HttpGet("ping-auth")]
    [Authorize]
    public IActionResult PingAuth() => Ok("pong");

    [HttpGet("validate")]
    [AllowAnonymous]
    public IActionResult Validate([FromHeader(Name = "Authorization")] string? authHeader)
    {
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { valid = false, error = "Missing Authorization header" });
        }
        var token = authHeader[7..].Trim();
        var handler = new JwtSecurityTokenHandler();
        var parameters = _jwtTokenService.GetValidationParameters();
        try
        {
            handler.ValidateToken(token, parameters, out _);
            return Ok(new { valid = true });
        }
        catch (Exception ex)
        {
            return Ok(new { valid = false, error = ex.Message });
        }
    }
}
