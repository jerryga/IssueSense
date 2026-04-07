# Role Access Presentation Table

This version is formatted for presentations, demos, and stakeholder reviews.

## System Roles

| Role | Main Purpose |
|---|---|
| `support_admin` | Full operational control of complaint handling |
| `analyst` | Read-only access for monitoring and analysis |
| `triage_officer` | Intake, routing, prioritization, and early-stage handling |
| `case_manager` | Full complaint lifecycle handling and resolution |
| `ai_reviewer` | AI result review, re-analysis, and AI-focused commenting |

## Functional Access Table

| Feature / Action | support_admin | analyst | triage_officer | case_manager | ai_reviewer |
|---|---|---|---|---|---|
| Login / Logout | Yes | Yes | Yes | Yes | Yes |
| View dashboard | Yes | Yes | Yes | Yes | Yes |
| View complaint list | Yes | Yes | Yes | Yes | Yes |
| View complaint details | Yes | Yes | Yes | Yes | Yes |
| Create complaint | Yes | No | Yes | Yes | No |
| Add comment | Yes | No | Yes | Yes | Yes |
| Assign complaint owner | Yes | No | Yes | Yes | No |
| Re-analyze complaint | Yes | No | No | Yes | Yes |
| Update complaint status | Yes | No | Limited | Yes | No |
| View AI suggested actions | Yes | Yes | Yes | Yes | Yes |
| Access analytics dashboard | Yes | Yes | Yes | Yes | Yes |

## Status Transition Rules

| Role | Allowed Status Actions |
|---|---|
| `support_admin` | Full status control |
| `case_manager` | Full complaint workflow handling |
| `triage_officer` | Only `New -> InProgress` |
| `analyst` | No status updates |
| `ai_reviewer` | No status updates |

## Assignment Permissions

| Role | Can Assign Owner? |
|---|---|
| `support_admin` | Yes |
| `triage_officer` | Yes |
| `case_manager` | Yes |
| `analyst` | No |
| `ai_reviewer` | No |

## AI Review Permissions

| Role | Can Re-analyze? | Can Add AI Review Comment? | Can Change Workflow Status? |
|---|---|---|---|
| `support_admin` | Yes | Yes | Yes |
| `case_manager` | Yes | Yes | Yes |
| `ai_reviewer` | Yes | Yes | No |
| `triage_officer` | No | Yes | Limited |
| `analyst` | No | No | No |

## Demo Accounts

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | `support_admin` |
| `analyst` | `Analyst@123` | `analyst` |
| `triage` | `Triage@123` | `triage_officer` |
| `casemanager` | `Case@123` | `case_manager` |
| `aireviewer` | `Review@123` | `ai_reviewer` |

## Notes

- The system intentionally does **not** use a `super_admin` role.
- Operational control is centered on `support_admin`.
- `analyst` is strictly read-only.
- AI action suggestions can be reviewed and turned into actual assignments.
