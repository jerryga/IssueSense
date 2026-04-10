using System.Security.Claims;
using IssueSense.Application.DTOs.Auth;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Web.Controllers;
using IssueSense.Web.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IssueSense.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public void Login_Get_WhenAuthenticated_RedirectsToDashboard()
    {
        var controller = CreateController(new Mock<IUserService>().Object, authenticated: true);

        var result = controller.Login();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Dashboard", redirect.ControllerName);
    }

    [Fact]
    public async Task Login_Post_InvalidModel_ReturnsView()
    {
        var controller = CreateController(new Mock<IUserService>().Object);
        controller.ModelState.AddModelError(nameof(LoginViewModel.UserName), "Required");

        var result = await controller.Login(new LoginViewModel(), cancellationToken: CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Account/Login.cshtml", view.ViewName);
    }

    [Fact]
    public async Task Login_Post_FailedLogin_AddsModelErrorAndReturnsView()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResultDto
            {
                Success = false,
                ErrorMessage = "Invalid username or password."
            });

        var controller = CreateController(serviceMock.Object);

        var result = await controller.Login(new LoginViewModel
        {
            UserName = "admin",
            Password = "wrong-password"
        }, cancellationToken: CancellationToken.None);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal("~/Views/Account/Login.cshtml", view.ViewName);
    }

    private static AuthController CreateController(IUserService service, bool authenticated = false)
    {
        var controller = new AuthController(service, NullLogger<AuthController>.Instance);
        var httpContext = new DefaultHttpContext();
        httpContext.User = authenticated
            ? new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], "TestAuth"))
            : new ClaimsPrincipal(new ClaimsIdentity());
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        return controller;
    }
}
