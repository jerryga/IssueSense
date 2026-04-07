# Rubric Grading Sheet

This grading sheet summarizes the current implementation status of the IssueSense project against the provided rubric.

## Project

- **Application Name:** IssueSense
- **Framework:** ASP.NET Core MVC
- **Database:** MongoDB
- **Architecture:** Layered MVC with separation between Web, Application, Domain, and Infrastructure

## Grading Table

| # | Rubric Criterion | Current Status | Evidence | Assessment |
|---|---|---|---|---|
| 1 | All CRUD operations (Create, Read, Update, Delete) are fully functional and seamlessly implemented with no errors. | Partially implemented | Complaint create/read/update flows are implemented in [ComplaintController.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Controllers/ComplaintController.cs) and [ComplaintService.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Application/Services/ComplaintService.cs). Delete exists in repository interfaces and implementations, but is not exposed end to end in the MVC UI/controllers. | **Partially Meets** |
| 2 | Implements essential security features like user authentication, input validation, and password hashing. Code adheres to security best practices. | Implemented | Authentication and cookie security are configured in [Program.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Program.cs). Login/logout are in [AuthController.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Controllers/AuthController.cs). Password hashing uses PBKDF2 in [PasswordSecurity.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Application/Security/PasswordSecurity.cs). Validation is applied across view models in [IssueSense.Web/Models](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Models). | **Meets** |
| 3 | Database integration is seamless, and all data operations are accurate, efficient, and error-free. | Implemented | MongoDB integration is handled by [MongoDbContext.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure/Contexts/MongoDbContext.cs), [ComplaintRepository.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure/Repositories/ComplaintRepository.cs), and [UserRepository.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure/Repositories/UserRepository.cs). Build and tests are passing. | **Meets** |
| 4 | The application has a clean, intuitive, and responsive interface that enhances user experience. | Implemented | Tailwind-based Razor UI across dashboard, complaint pages, and user management. Key views include [Dashboard/Index.cshtml](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Views/Dashboard/Index.cshtml), [Complaints/Index.cshtml](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Views/Complaints/Index.cshtml), [Complaints/Details.cshtml](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Views/Complaints/Details.cshtml), and [Users/Index.cshtml](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Views/Users/Index.cshtml). | **Meets** |
| 5 | Code is clean, well-organized, and adheres to standard conventions with clear comments and documentation. | Implemented | The solution is separated into [IssueSense.Web](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web), [IssueSense.Application](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Application), [IssueSense.Domain](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Domain), [IssueSense.Infrastructure](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure), and [IssueSense.Tests](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Tests). Supporting documentation exists in [docs](/Users/chason/Documents/GitHub/IssueSense/docs). | **Meets** |
| 6 | Application handles errors gracefully with informative feedback to the user. | Implemented | Error handling includes `TempData` feedback in [ComplaintController.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Controllers/ComplaintController.cs), login errors in [AuthController.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Controllers/AuthController.cs), and a global exception handler in [Program.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Program.cs). | **Meets** |

## Build and Test Status

| Check | Result |
|---|---|
| Solution Build | Passed |
| Automated Tests | Passed |
| Current Test Count | 8 passed, 0 failed |

## Screenshot Evidence

Complaint detail page:

![Complaint detail UI](/Users/chason/Desktop/Screenshot%202026-04-07%20at%209.59.21%E2%80%AFAM.png)

## Overall Result

| Overall Category | Result |
|---|---|
| Functional completeness | Strong, with one remaining CRUD gap |
| Security | Strong |
| Database integration | Strong |
| UI/UX | Strong |
| Code organization | Strong |
| Error handling | Strong |

## Final Note

The current project performs well across the rubric and is especially strong in security, architecture, database integration, and user workflow design.

The main remaining improvement for full rubric completion is:

- implement complete delete functionality in the MVC layer for complaints and user accounts
