using System.Security.Claims;
using IssueSense.Application.DTOs.Complaints;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Domain.Common;
using IssueSense.Domain.Enums;
using IssueSense.Web.Controllers;
using IssueSense.Web.Models.Complaints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace IssueSense.Tests.Controllers;

public sealed class ComplaintControllerTests
{
    [Fact]
    public async Task Details_ReturnsNotFound_WhenComplaintDoesNotExist()
    {
        var serviceMock = new Mock<IComplaintService>();
        serviceMock.Setup(x => x.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ComplaintDetailDto?)null);

        var controller = CreateController(serviceMock.Object);

        var result = await controller.Details("missing", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddComment_InvalidModel_SetsErrorAndRedirectsToDetails()
    {
        var serviceMock = new Mock<IComplaintService>();
        var controller = CreateController(serviceMock.Object);
        controller.ModelState.AddModelError(nameof(ComplaintCommentCreateViewModel.Message), "Required");

        var result = await controller.AddComment(new ComplaintCommentCreateViewModel
        {
            ComplaintId = "507f1f77bcf86cd799439011",
            Message = string.Empty
        }, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("507f1f77bcf86cd799439011", redirect.RouteValues?["id"]);
        Assert.Equal("Comment must be at least 3 characters long.", controller.TempData["ErrorMessage"]);
        serviceMock.Verify(x => x.AddCommentAsync(It.IsAny<ComplaintCommentCreateDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_TriageOfficer_InvalidTransition_ShowsErrorAndSkipsUpdate()
    {
        var serviceMock = new Mock<IComplaintService>();
        serviceMock.Setup(x => x.GetByIdAsync("507f1f77bcf86cd799439011", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ComplaintDetailDto
            {
                Id = "507f1f77bcf86cd799439011",
                Status = ComplaintStatus.InProgress
            });

        var controller = CreateController(serviceMock.Object, RoleNames.TriageOfficer);

        var result = await controller.UpdateStatus(new ComplaintStatusUpdateViewModel
        {
            ComplaintId = "507f1f77bcf86cd799439011",
            Status = ComplaintStatus.Resolved
        }, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("507f1f77bcf86cd799439011", redirect.RouteValues?["id"]);
        Assert.Equal("Triage officers can only move complaints from New to InProgress.", controller.TempData["ErrorMessage"]);
        serviceMock.Verify(x => x.UpdateStatusAsync(It.IsAny<ComplaintStatusUpdateDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Reanalyze_WithoutComplaintId_RedirectsToIndex()
    {
        var serviceMock = new Mock<IComplaintService>();
        var controller = CreateController(serviceMock.Object, RoleNames.AiReviewer);

        var result = await controller.Reanalyze(string.Empty, CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        serviceMock.Verify(x => x.ReanalyzeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Archive_WithoutComplaintId_RedirectsToIndex()
    {
        var serviceMock = new Mock<IComplaintService>();
        var controller = CreateController(serviceMock.Object, RoleNames.SupportAdmin);

        var result = await controller.Archive(" ", CancellationToken.None);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        serviceMock.Verify(x => x.ArchiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ComplaintController CreateController(IComplaintService service, string? role = null)
    {
        var controller = new ComplaintController(service);
        var httpContext = new DefaultHttpContext();
        httpContext.User = CreatePrincipal(role);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        return controller;
    }

    private static ClaimsPrincipal CreatePrincipal(string? role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "demo.user"),
            new(ClaimTypes.GivenName, "Demo User")
        };

        if (!string.IsNullOrWhiteSpace(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }
}
