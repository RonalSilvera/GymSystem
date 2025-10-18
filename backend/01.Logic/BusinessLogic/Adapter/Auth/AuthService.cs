using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusinessLogic.Ports;
using Domain.Interface;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BusinessLogic.Adapter.Auth;

public class AuthService(IUserRepository repository, IUnitOfWork unitOfWork, IConfiguration config) : IAuthService
{
    private readonly IUserRepository _repository = repository;
    private readonly IUnitOfWork _uow = unitOfWork;
    private readonly IConfiguration _config = config;

    public async Task<string?> AuthenticateAsync(string email, string password)
    {
        const int StatusActivo = 1;
        var user = await _repository.FindByEmail(email);
        if (user == null || user.StatusId != StatusActivo) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        var rawKey = _config["Jwt:Key"] ?? _config["Jwt:secretKey"];
        if (string.IsNullOrWhiteSpace(rawKey))
        {
            throw new InvalidOperationException("No se encontró la clave de firma JWT en la configuración.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(rawKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            config["Jwt:issuer"],
            config["Jwt:audience"],
            [
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            ],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task LogoutAsync()
    {
        // JWT tokens are stateless so logout is handled on the client side.
        // This method exists for symmetry and future extension.
        return Task.CompletedTask;
    }
}
