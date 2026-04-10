namespace IssueSense.Domain.Common;

public static class RoleNames
{
    public const string SupportAdmin = "support_admin";
    public const string UserAdmin = "user_admin";
    public const string Analyst = "analyst";
    public const string TriageOfficer = "triage_officer";
    public const string CaseManager = "case_manager";
    public const string AiReviewer = "ai_reviewer";

    public const string AllRoles = $"{SupportAdmin},{UserAdmin},{Analyst},{TriageOfficer},{CaseManager},{AiReviewer}";
    public const string ComplaintCreators = $"{SupportAdmin},{TriageOfficer},{CaseManager}";
    public const string CommentAuthors = $"{SupportAdmin},{TriageOfficer},{CaseManager},{AiReviewer}";
    public const string StatusEditors = $"{SupportAdmin},{TriageOfficer},{CaseManager}";
    public const string AnalysisReviewers = $"{CaseManager},{AiReviewer}";
    public const string AssignmentEditors = $"{SupportAdmin},{TriageOfficer},{CaseManager}";
    public const string UserManagers = $"{UserAdmin}";
    public const string ArchiveManagers = $"{CaseManager}";
}
