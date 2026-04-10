using IssueSense.Application.DTOs.Complaints;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Domain.Common;
using IssueSense.Web.Extensions;
using IssueSense.Web.Models.Complaints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueSense.Web.Controllers;

[Authorize(Roles = RoleNames.AllRoles)]
public sealed class ComplaintController(IComplaintService complaintService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ComplaintQueryDto query, CancellationToken cancellationToken)
    {
        var complaints = await complaintService.GetAllAsync(query, cancellationToken);
        return View("~/Views/Complaints/Index.cshtml", new ComplaintIndexPageViewModel
        {
            Query = query,
            Complaints = complaints
        });
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.ComplaintCreators)]
    public IActionResult Create() => View("~/Views/Complaints/Create.cshtml", new ComplaintCreateViewModel());

    [HttpPost]
    [Authorize(Roles = RoleNames.ComplaintCreators)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ComplaintCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Complaints/Create.cshtml", model);
        }

        await complaintService.CreateAsync(new ComplaintCreateDto
        {
            Title = model.Title,
            Description = model.Description,
            CustomerName = model.CustomerName,
            CustomerEmail = model.CustomerEmail,
            CreatedByUserName = User.GetUserName()
        }, cancellationToken);

        TempData["SuccessMessage"] = "Complaint created and analyzed successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id, CancellationToken cancellationToken)
    {
        var complaint = await complaintService.GetByIdAsync(id, cancellationToken);
        if (complaint is null)
        {
            return NotFound();
        }

        return View("~/Views/Complaints/Details.cshtml", new ComplaintDetailsPageViewModel
        {
            Complaint = complaint,
            NewComment = new ComplaintCommentCreateViewModel { ComplaintId = complaint.Id },
            StatusUpdate = new ComplaintStatusUpdateViewModel { ComplaintId = complaint.Id, Status = complaint.Status },
            AssignmentUpdate = new ComplaintAssignmentUpdateViewModel
            {
                ComplaintId = complaint.Id,
                AssignedOwner = complaint.AssignedOwner
            }
        });
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.StatusEditors)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(ComplaintStatusUpdateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
        }

        var complaint = await complaintService.GetByIdAsync(model.ComplaintId, cancellationToken);
        if (complaint is null)
        {
            return NotFound();
        }

        var isSupportAdmin = User.IsInRole(RoleNames.SupportAdmin);
        var isCaseManager = User.IsInRole(RoleNames.CaseManager);
        var isTriageOfficer = User.IsInRole(RoleNames.TriageOfficer);

        if (isTriageOfficer && (complaint.Status != Domain.Enums.ComplaintStatus.New || model.Status != Domain.Enums.ComplaintStatus.InProgress))
        {
            TempData["ErrorMessage"] = "Triage officers can only move complaints from New to InProgress.";
            return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
        }

        if (!isSupportAdmin && !isCaseManager && !isTriageOfficer)
        {
            return Forbid();
        }

        await complaintService.UpdateStatusAsync(new ComplaintStatusUpdateDto
        {
            ComplaintId = model.ComplaintId,
            Status = model.Status
        }, cancellationToken);

        TempData["SuccessMessage"] = "Complaint status updated.";
        return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.CommentAuthors)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(ComplaintCommentCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Comment must be at least 3 characters long.";
            return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
        }

        await complaintService.AddCommentAsync(new ComplaintCommentCreateDto
        {
            ComplaintId = model.ComplaintId,
            AuthorUserName = User.GetDisplayName(),
            Message = model.Message
        }, cancellationToken);

        TempData["SuccessMessage"] = "Comment added.";
        return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.AssignmentEditors)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAssignment(ComplaintAssignmentUpdateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please select an owner to assign.";
            return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
        }

        await complaintService.UpdateAssignmentAsync(new ComplaintAssignmentUpdateDto
        {
            ComplaintId = model.ComplaintId,
            AssignedOwner = model.AssignedOwner
        }, cancellationToken);

        TempData["SuccessMessage"] = "Complaint assignment updated.";
        return RedirectToAction(nameof(Details), new { id = model.ComplaintId });
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.AnalysisReviewers)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reanalyze(string complaintId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(complaintId))
        {
            return RedirectToAction(nameof(Index));
        }

        await complaintService.ReanalyzeAsync(complaintId, cancellationToken);
        TempData["SuccessMessage"] = "Complaint re-analyzed successfully.";
        return RedirectToAction(nameof(Details), new { id = complaintId });
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.ArchiveManagers)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(string complaintId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(complaintId))
        {
            return RedirectToAction(nameof(Index));
        }

        await complaintService.ArchiveAsync(complaintId, User.GetDisplayName(), cancellationToken);
        TempData["SuccessMessage"] = "Complaint archived successfully.";
        return RedirectToAction(nameof(Index));
    }
}
