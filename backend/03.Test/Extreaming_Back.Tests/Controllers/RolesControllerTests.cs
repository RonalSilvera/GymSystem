using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Ports;
using Infrastructure.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class RolesControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var svc = new Mock<IRoleService>();
        svc.Setup(s => s.All()).ReturnsAsync(new List<RoleDto>());
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateRoleModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var controller = new API.Controllers.RolesController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.GetAll();
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ReturnsOk()
    {
        var svc = new Mock<IRoleService>();
        svc.Setup(s => s.Detail(It.IsAny<Guid>())).ReturnsAsync(new RoleDto());
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateRoleModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var controller = new API.Controllers.RolesController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.GetById(Guid.NewGuid());
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsOk()
    {
        var svc = new Mock<IRoleService>();
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateRoleModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var controller = new API.Controllers.RolesController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.Create(new RoleDto());
        Assert.IsType<OkResult>(result);
    }
}
