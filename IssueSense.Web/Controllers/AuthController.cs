using System.Security.Claims;
using IssueSense.Application.DTOs.Auth;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Web.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IssueSense.Web.Controllers;

public sealed class AuthController(
    IUserService userService,
    ILogger<AuthController> logger) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View("~/Views/Account/Login.cshtml", new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Account/Login.cshtml", model);
        }

        var result = await userService.LoginAsync(new LoginRequestDto
        {
            UserName = model.UserName,
            Password = model.Password
        }, cancellationToken);

        if (!result.Success || result.User is null)
        {
            logger.LogWarning(
                "Interactive login rejected for user {UserName}. Reason: {Reason}",
                model.UserName.Trim(),
                result.ErrorMessage ?? "Unknown");
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid username or password.");
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Account/Login.cshtml", model);
        }

        var user = result.User;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.GivenName, user.DisplayName),
            new(ClaimTypes.Role, user.Role)
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        logger.LogInformation("Interactive login succeeded for user {UserName}.", user.UserName);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        logger.LogInformation("User {UserName} logged out.", User.Identity?.Name ?? "unknown");
        return RedirectToAction(nameof(Login));
    }
}
