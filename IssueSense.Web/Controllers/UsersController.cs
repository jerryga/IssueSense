using IssueSense.Application.DTOs.Users;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Domain.Common;
using IssueSense.Web.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueSense.Web.Controllers;

[Authorize(Roles = RoleNames.UserManagers)]
public sealed class UsersController(IUserService userService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View("~/Views/Users/Index.cshtml", new UserManagementPageViewModel
        {
            Users = await userService.GetAllAsync(cancellationToken),
            NewUser = new UserCreateViewModel()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Users/Index.cshtml", new UserManagementPageViewModel
            {
                Users = await userService.GetAllAsync(cancellationToken),
                NewUser = model
            });
        }

        var result = await userService.CreateAsync(new UserCreateDto
        {
            UserName = model.UserName,
            DisplayName = model.DisplayName,
            Password = model.Password,
            Role = model.Role
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(nameof(model.UserName), result.ErrorMessage ?? "Unable to create the user.");
            return View("~/Views/Users/Index.cshtml", new UserManagementPageViewModel
            {
                Users = await userService.GetAllAsync(cancellationToken),
                NewUser = model
            });
        }

        TempData["SuccessMessage"] = "User account created successfully.";
        return RedirectToAction(nameof(Index));
    }
}
