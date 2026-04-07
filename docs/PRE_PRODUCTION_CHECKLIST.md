# Pre-Production Checklist

This checklist summarizes what has been completed for deployment hardening and what still remains before a production rollout of IssueSense.

## Completed

- `SeedData` now defaults to `false` in [appsettings.json](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/appsettings.json)
- startup seeding now only runs when both of these are true:
  - the app is running in `Development`
  - `SeedData=true`
- forwarded headers are configured for reverse-proxy hosting in [Program.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Program.cs)
- secure cookie settings remain enabled for production hosting
- MongoDB indexes are created automatically on startup in [MongoDbContext.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure/Contexts/MongoDbContext.cs)
- a unique index is now enforced for usernames
- operational complaint indexes are now created for:
  - created date
  - archive state
  - status
  - escalation status
  - assigned owner

## Ready For Deployment

- authentication and role-based authorization
- complaint create, read, update, archive workflow
- AI analysis with OpenAI or mock fallback
- manual complaint re-analysis
- assignment workflow
- internal user creation
- dashboard analytics
- archive visibility for support admins

## Still Recommended Before Production

- add user edit, deactivate, and password reset flows
- add `IsActive`, `CreatedAt`, and `LastLoginAt` to the active runtime user model and enforce them in login logic
- add audit history for:
  - status changes
  - assignment changes
  - archive actions
  - AI re-analysis actions
- add rate limiting or account lockout for repeated failed logins
- add production logging and monitoring around OpenAI failures and fallback behavior
- add pagination for large complaint datasets
- add browser or end-to-end tests for role-based workflows
- add Mongo-backed integration tests for real persistence behavior
- replace the plain-text 500 response with a friendlier production error experience

## Deployment Settings

For production environments such as Render:

- keep `SeedData=false`
- set MongoDB values through environment variables
- set OpenAI values through environment variables
- run behind HTTPS
- use a hosted MongoDB instance with backups enabled

## Verification

Latest local verification:

- `dotnet build IssueSense.slnx -p:UseSharedCompilation=false -maxcpucount:1 -nr:false`
- `dotnet test IssueSense.slnx -p:UseSharedCompilation=false -maxcpucount:1 -nr:false`

Result:

- build passed
- 9 tests passed
- 0 tests failed
