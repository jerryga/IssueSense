using System.Security.Claims;
using IssueSense.Domain.Common;

namespace IssueSense.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

    public static string GetDisplayName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.GivenName) ?? principal.GetUserName();

    public static string GetRoleName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public static string GetRoleDisplayName(this ClaimsPrincipal principal) =>
        principal.GetRoleName() switch
        {
            RoleNames.SupportAdmin => "Support Admin",
            RoleNames.UserAdmin => "User Admin",
            RoleNames.Analyst => "Analyst",
            RoleNames.TriageOfficer => "Triage Officer",
            RoleNames.CaseManager => "Case Manager",
            RoleNames.AiReviewer => "AI Reviewer",
            _ => "User"
        };
}
