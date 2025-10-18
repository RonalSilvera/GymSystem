using BusinessLogic.Adapter.Auth;
using BusinessLogic.Ports;
using Domain.Interface;
using Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _repo;
    private readonly Mock<IUnitOfWork> _uow;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _repo = new Mock<IUserRepository>();
        _uow = new Mock<IUnitOfWork>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"Jwt:Key","12345678901234567890123456789012"},
                {"Jwt:secretKey","12345678901234567890123456789012"},
                {"Jwt:issuer","issuer"},
                {"Jwt:audience","aud"}
            }).Build();
        _service = new AuthService(_repo.Object, _uow.Object, config);
    }

    [Fact]
    public async Task Authenticate_ReturnsToken_WhenCredentialsValid()
    {
        var user = new Domain.Entity.Users
        {
            UserId = Guid.NewGuid(),
            Email = "test@a.com",
            PasswordHash = "$2b$10$X.kYLwLVsca6YGe4GrO8gu7z4Qd9ccbunSdP/5i1lO7oaa1jqL2Kq",
            Role = "admin",
            StatusId = 1
        };
        _repo.Setup(r => r.FindByEmail(user.Email)).ReturnsAsync(user);

        var token = await _service.AuthenticateAsync(user.Email, "p");

        Assert.NotNull(token);
    }

    [Fact]
    public async Task Authenticate_ReturnsNull_WhenInvalid()
    {
        _repo.Setup(r => r.FindByEmail(It.IsAny<string>())).ReturnsAsync((Domain.Entity.Users?)null);

        var token = await _service.AuthenticateAsync("no", "pass");

        Assert.Null(token);
    }

    [Fact]
    public async Task Logout_Completes()
    {
        await _service.LogoutAsync();
    }
}
