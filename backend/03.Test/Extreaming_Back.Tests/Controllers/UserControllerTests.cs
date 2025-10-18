using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Ports;
using Infrastructure.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System.Security.Claims;
using Domain.Common.Enums;
using System.Linq;

public class UserControllerTests
{
    [Fact]
    public async Task Register_SuperAdminCanCreateAdmin()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.Create(It.IsAny<UserDto>())).ReturnsAsync(new UserDto());
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.SuperAdmin) }));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        var result = await controller.Register(new UserDto { Role = RoleNames.Admin });
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Register_AdminCannotCreateSuperAdmin()
    {
        var svc = new Mock<IUserService>();
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Admin) }));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        var result = await controller.Register(new UserDto { Role = RoleNames.SuperAdmin });
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithUsers()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.All()).ReturnsAsync(new List<UserDto> { new(), new() });
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.GetAll() as OkObjectResult;
        var list = Assert.IsAssignableFrom<IEnumerable<UserDto>>(result!.Value);
        Assert.Equal(2, list.Count());
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.Detail(It.IsAny<Guid>())).ReturnsAsync((UserDto?)null);
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.GetById(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.Detail(It.IsAny<Guid>())).ReturnsAsync(new UserDto());
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.GetById(Guid.NewGuid());
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.Detail(It.IsAny<Guid>())).ReturnsAsync(new UserDto { Role = RoleNames.Operator });
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.SuperAdmin) }));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        var result = await controller.Delete(Guid.NewGuid());
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ByOperator_ReturnsForbid()
    {
        var svc = new Mock<IUserService>();
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Operator) }));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        var result = await controller.Delete(Guid.NewGuid());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Delete_AdminDeletingAdmin_ReturnsForbid()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.Detail(It.IsAny<Guid>())).ReturnsAsync(new UserDto { Role = RoleNames.Admin });
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Admin) }));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        var result = await controller.Delete(Guid.NewGuid());
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Delete_AdminDeletingOperator_ReturnsNoContent()
    {
        var svc = new Mock<IUserService>();
        svc.Setup(s => s.Detail(It.IsAny<Guid>())).ReturnsAsync(new UserDto { Role = RoleNames.Operator });
        svc.Setup(s => s.Delete(It.IsAny<Guid>())).Returns(Task.CompletedTask);
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, RoleNames.Admin) }));
        var controller = new API.Controllers.UsersController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
        var result = await controller.Delete(Guid.NewGuid());
        Assert.IsType<NoContentResult>(result);
        svc.Verify(s => s.Delete(It.IsAny<Guid>()), Times.Once);
    }

}

