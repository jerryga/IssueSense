# Role Access Guide

This document explains the role-based access control (RBAC) model used in the AI-Based Complaint and Sentiment Analysis System.

## Roles

The system uses these five operational roles:

- `support_admin`
- `analyst`
- `triage_officer`
- `case_manager`
- `ai_reviewer`

There is no `super_admin` role in this application.

## Role Purpose

### `support_admin`

Full operational access across the complaint workflow.

Typical responsibilities:

- create complaints
- update complaint status
- assign complaint owners
- add comments
- re-analyze complaints with AI
- review dashboards and analytics

### `analyst`

Read-only access for monitoring and analysis.

Typical responsibilities:

- view dashboard
- view complaint list
- view complaint details
- review AI outputs and trends

Restrictions:

- cannot create complaints
- cannot update status
- cannot assign owners
- cannot add comments
- cannot trigger re-analysis

### `triage_officer`

Handles initial intake, routing, and early-stage prioritization.

Typical responsibilities:

- create complaints
- assign owners
- add comments
- review dashboards and complaint details

Status limitation:

- can only move complaints from `New` to `InProgress`

### `case_manager`

Owns complaint handling through the operational lifecycle.

Typical responsibilities:

- create complaints
- assign owners
- add comments
- update complaint status
- re-analyze complaints
- manage escalated complaints

### `ai_reviewer`

Focuses on AI output review and refinement support.

Typical responsibilities:

- view complaints and dashboard
- add AI review comments
- trigger complaint re-analysis
- review suggested actions and confidence scores

Restrictions:

- cannot create complaints
- cannot assign owners
- cannot update workflow status

## Access Matrix

| Feature / Action | support_admin | analyst | triage_officer | case_manager | ai_reviewer |
|---|---|---|---|---|---|
| Login / Logout | Yes | Yes | Yes | Yes | Yes |
| View dashboard | Yes | Yes | Yes | Yes | Yes |
| View complaint list | Yes | Yes | Yes | Yes | Yes |
| View complaint details | Yes | Yes | Yes | Yes | Yes |
| Create complaint | Yes | No | Yes | Yes | No |
| Add comment | Yes | No | Yes | Yes | Yes |
| Assign owner | Yes | No | Yes | Yes | No |
| Re-analyze complaint | Yes | No | No | Yes | Yes |
| Update status | Yes | No | Limited | Yes | No |
| View AI suggested actions | Yes | Yes | Yes | Yes | Yes |
| View analytics dashboard | Yes | Yes | Yes | Yes | Yes |

## Status Update Rules

### `support_admin`

Can move a complaint through any valid operational status.

### `case_manager`

Can manage the normal complaint lifecycle:

- `New`
- `InProgress`
- `Resolved`
- `Closed`

### `triage_officer`

Can only perform the early-stage transition:

- `New -> InProgress`

If a triage officer tries to make any other status change, the system blocks it.

### `analyst`

Cannot update status.

### `ai_reviewer`

Cannot update status.

## Assignment Workflow

The application supports explicit complaint assignment.

Operational roles can assign a complaint to:

- `triage_officer`
- `case_manager`
- `support_admin`
- `ai_reviewer`

Assignment editors:

- `support_admin`
- `triage_officer`
- `case_manager`

## AI Review Workflow

The AI service produces:

- sentiment
- category
- urgency
- confidence
- action-required indicator
- suggested action items with `@owner`

The complaint can then be:

- manually re-analyzed
- assigned to a responsible owner
- escalated if the rules require it

Roles involved in AI review:

- `support_admin`
- `case_manager`
- `ai_reviewer`

## Demo Accounts

The system seeds the following demo users:

- `admin / Admin@123` -> `support_admin`
- `analyst / Analyst@123` -> `analyst`
- `triage / Triage@123` -> `triage_officer`
- `casemanager / Case@123` -> `case_manager`
- `aireviewer / Review@123` -> `ai_reviewer`

## Notes For Demonstration

When demonstrating the system, a good order is:

1. Log in as `analyst` and show read-only access.
2. Log in as `triage_officer` and show complaint creation, comment entry, assignment, and limited status transition.
3. Log in as `case_manager` and show full complaint handling plus re-analysis.
4. Log in as `ai_reviewer` and show AI review comments and manual re-analysis.
5. Log in as `support_admin` and show full operational access.

## Related Files

The RBAC implementation is reflected in these project files:

- `/Users/chason/Documents/GitHub/IssueSense/IssueSense.Domain/Common/RoleNames.cs`
- `/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Controllers/AuthController.cs`
- `/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Controllers/ComplaintController.cs`
- `/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Controllers/DashboardController.cs`
- `/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Views/Complaints/Details.cshtml`
- `/Users/chason/Documents/GitHub/IssueSense/IssueSense.Application/Services/UserService.cs`
