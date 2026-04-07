# Role Access Summary

This is a short summary of the role-based access model used in the AI-Based Complaint and Sentiment Analysis System.

## Roles

- `support_admin`
- `analyst`
- `triage_officer`
- `case_manager`
- `ai_reviewer`

There is no `super_admin` role in this system.

## Role Overview

### `support_admin`

Full operational access.

Can:

- create complaints
- assign owners
- update status
- add comments
- trigger AI re-analysis
- access dashboard and analytics

### `analyst`

Read-only role.

Can:

- view dashboard
- view complaint list
- view complaint details
- review AI outputs

Cannot:

- create complaints
- assign owners
- update status
- add comments
- re-analyze complaints

### `triage_officer`

Initial intake and routing role.

Can:

- create complaints
- assign owners
- add comments
- view analytics and complaints

Status rule:

- can only move `New -> InProgress`

### `case_manager`

Complaint lifecycle handler.

Can:

- create complaints
- assign owners
- add comments
- update complaint status
- trigger AI re-analysis

### `ai_reviewer`

AI review support role.

Can:

- view complaints and dashboard
- add AI review comments
- trigger AI re-analysis
- review suggested action items

Cannot:

- create complaints
- assign owners
- update workflow status

## Quick Access Matrix

| Action | support_admin | analyst | triage_officer | case_manager | ai_reviewer |
|---|---|---|---|---|---|
| View dashboard | Yes | Yes | Yes | Yes | Yes |
| View complaints | Yes | Yes | Yes | Yes | Yes |
| Create complaint | Yes | No | Yes | Yes | No |
| Add comment | Yes | No | Yes | Yes | Yes |
| Assign owner | Yes | No | Yes | Yes | No |
| Re-analyze complaint | Yes | No | No | Yes | Yes |
| Update status | Yes | No | Limited | Yes | No |

## Demo Users

- `admin / Admin@123`
- `analyst / Analyst@123`
- `triage / Triage@123`
- `casemanager / Case@123`
- `aireviewer / Review@123`

## Full Documentation

For the full explanation, use:

[ROLE_ACCESS_GUIDE.md](/Users/chason/Documents/GitHub/IssueSense/docs/ROLE_ACCESS_GUIDE.md)
