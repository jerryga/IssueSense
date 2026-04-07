using IssueSense.Application.Interfaces.Services;
using IssueSense.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueSense.Web.Controllers;

[Authorize(Roles = RoleNames.AllRoles)]
public sealed class DashboardController(IComplaintService complaintService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var summary = await complaintService.GetDashboardSummaryAsync(cancellationToken);
        return View(summary);
    }
}
