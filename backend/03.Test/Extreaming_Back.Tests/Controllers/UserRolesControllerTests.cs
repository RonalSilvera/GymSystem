using System;
using System.Threading.Tasks;
using BusinessLogic.Ports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class UserRolesControllerTests
{
    [Fact]
    public async Task AssignRole_ReturnsOk()
    {
        var svc = new Mock<IUserRoleService>();
        var fac = new Mock<IFactoryLogic>();
        fac.Setup(f => f.CreateUserRoleModule(It.IsAny<string>())).Returns((null!, null!, svc.Object));
        var controller = new API.Controllers.UserRolesController(fac.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        var result = await controller.AssignRole(Guid.NewGuid(), Guid.NewGuid());
        Assert.IsType<OkResult>(result);
    }
}
