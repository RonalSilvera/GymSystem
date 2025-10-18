using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Services;
using API.Models.Auth;
using Xunit;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ReturnsOk_WhenValid()
    {
        var repo = new Mock<Domain.Interface.IUserRepository>();
        repo.Setup(r => r.FindByEmail(It.IsAny<string>())).ReturnsAsync(new Domain.Entity.Users
        {
            UserId = Guid.NewGuid(),
            Email = "e",
            Role = "admin",
            PasswordHash = "h",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateAuthModule(It.IsAny<string>())).Returns((repo.Object, null!, null!));
        var tokenSvc = new Mock<IJwtTokenService>();
        tokenSvc.Setup(t => t.GenerateToken(It.IsAny<Domain.Entity.Users>())).Returns(("tok", DateTime.UtcNow.AddHours(1)));
        var pwdSvc = new Mock<IPasswordService>();
        pwdSvc.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        var controller = new API.Controllers.AuthController(fac.Object, tokenSvc.Object, pwdSvc.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.Login(new LoginRequest { Email = "e", Password = "p" });
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalid()
    {
        var repo = new Mock<Domain.Interface.IUserRepository>();
        repo.Setup(r => r.FindByEmail(It.IsAny<string>())).ReturnsAsync((Domain.Entity.Users?)null);
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateAuthModule(It.IsAny<string>())).Returns((repo.Object, null!, null!));
        var tokenSvc = new Mock<IJwtTokenService>();
        var pwdSvc = new Mock<IPasswordService>();
        pwdSvc.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var controller = new API.Controllers.AuthController(fac.Object, tokenSvc.Object, pwdSvc.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.Login(new LoginRequest { Email = "e", Password = "p" });
        Assert.IsType<UnauthorizedResult>(result.Result);
    }
}
