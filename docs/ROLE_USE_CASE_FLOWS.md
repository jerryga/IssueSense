# Role Use-Case Flows

This document shows the main workflow for each role in the AI-Based Complaint and Sentiment Analysis System.

## Overview

The system uses five operational roles:

- `support_admin`
- `analyst`
- `triage_officer`
- `case_manager`
- `ai_reviewer`

## High-Level Role Flow

```mermaid
flowchart TD
    A["Complaint Submitted"] --> B["AI Analysis Runs"]
    B --> C["Sentiment, Category, Urgency, Confidence"]
    C --> D["AI Suggested Actions with @owner"]
    D --> E["Complaint Visible in Dashboard and Complaint List"]

    E --> F["Triage Officer Reviews Intake"]
    F --> G["Assign Owner / Add Triage Comment"]
    G --> H["Move Status: New -> InProgress"]

    E --> I["Case Manager Works Complaint"]
    I --> J["Add Comments / Re-analyze / Update Status"]
    J --> K["Resolved or Closed"]

    E --> L["AI Reviewer Reviews AI Output"]
    L --> M["Add AI Review Comment / Trigger Re-analysis"]

    E --> N["Analyst Monitors Dashboard and Complaints"]
    N --> O["Read-Only Review of Status, Sentiment, Urgency, Escalation"]

    E --> P["Support Admin Oversees Entire Workflow"]
    P --> Q["Full Access: Assign, Comment, Re-analyze, Update Status"]
```

## Per-Role Flows

### 1. Support Admin

```mermaid
flowchart TD
    A["Login as support_admin"] --> B["Open Dashboard"]
    B --> C["Review complaint analytics"]
    C --> D["Open complaint details"]
    D --> E["Assign owner"]
    E --> F["Add comment"]
    F --> G["Trigger re-analysis if needed"]
    G --> H["Update complaint status"]
    H --> I["Monitor escalated complaints"]
```

### 2. Analyst

```mermaid
flowchart TD
    A["Login as analyst"] --> B["Open Dashboard"]
    B --> C["Review complaint analytics"]
    C --> D["Open complaint list"]
    D --> E["View complaint details"]
    E --> F["Review sentiment, urgency, escalation, actions"]
    F --> G["No edit actions available"]
```

### 3. Triage Officer

```mermaid
flowchart TD
    A["Login as triage_officer"] --> B["View new complaints"]
    B --> C["Review complaint details"]
    C --> D["Add triage comment"]
    D --> E["Assign owner"]
    E --> F["Move status from New to InProgress"]
    F --> G["Hand off to case manager or support admin"]
```

### 4. Case Manager

```mermaid
flowchart TD
    A["Login as case_manager"] --> B["Open assigned complaints"]
    B --> C["Review complaint and AI suggestions"]
    C --> D["Add operational comments"]
    D --> E["Assign or reassign owner"]
    E --> F["Trigger re-analysis if needed"]
    F --> G["Update status through workflow"]
    G --> H["Resolve or close complaint"]
```

### 5. AI Reviewer

```mermaid
flowchart TD
    A["Login as ai_reviewer"] --> B["Open complaint details"]
    B --> C["Inspect sentiment, urgency, confidence"]
    C --> D["Review suggested AI action items"]
    D --> E["Add AI review comment"]
    E --> F["Trigger manual re-analysis"]
    F --> G["Return complaint to operational team"]
```

## Practical Demo Sequence

Use this order when demonstrating the system:

1. `analyst`
   Show dashboard and read-only complaint access.

2. `triage_officer`
   Show complaint intake, assignment, comment entry, and limited status movement.

3. `case_manager`
   Show complaint lifecycle handling and resolution flow.

4. `ai_reviewer`
   Show AI review comments and manual re-analysis.

5. `support_admin`
   Show full end-to-end operational control.

## Key Message

The workflow is intentionally split by function:

- `analyst` observes
- `triage_officer` routes
- `case_manager` resolves
- `ai_reviewer` validates AI output
- `support_admin` oversees operations
