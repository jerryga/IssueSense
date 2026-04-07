# API and Configuration Guide

This document explains how to configure and run the AI-Based Complaint and Sentiment Analysis System, including MongoDB, OpenAI integration, startup settings, and the main application service/API flow.

## 1. Overview

The application is an ASP.NET Core MVC web system.

It uses:

- ASP.NET Core MVC
- Razor Views
- MongoDB
- OpenAI Responses API for AI analysis
- fallback mock AI logic when OpenAI is disabled

This project is not a public REST API project. It is an MVC application with server-side controllers and service-layer APIs inside the application.

## 2. Main Configuration File

Primary configuration file:

- [appsettings.json](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/appsettings.json)

This file contains:

- MongoDB settings
- OpenAI settings
- startup seed toggle
- standard ASP.NET Core logging settings

## 3. MongoDB Configuration

Current section:

```json
"MongoDb": {
  "ConnectionString": "mongodb://localhost:27017",
  "DatabaseName": "IssueSenseDb",
  "ComplaintsCollectionName": "complaints",
  "UsersCollectionName": "users"
}
```

### Fields

#### `MongoDb:ConnectionString`

MongoDB connection string.

Examples:

- `mongodb://localhost:27017`
- `mongodb://127.0.0.1:27017`
- `mongodb+srv://<user>:<password>@<cluster>/`

#### `MongoDb:DatabaseName`

Database used by the application.

Default:

- `IssueSenseDb`

#### `MongoDb:ComplaintsCollectionName`

Collection name for complaint documents.

Default:

- `complaints`

#### `MongoDb:UsersCollectionName`

Collection name for user documents.

Default:

- `users`

## 4. OpenAI Configuration

Current section:

```json
"OpenAI": {
  "Enabled": false,
  "ApiKey": "",
  "Model": "gpt-5.4-nano",
  "Endpoint": "https://api.openai.com/v1/responses",
  "UseMockFallback": true
}
```

### Fields

#### `OpenAI:Enabled`

Controls whether the application calls the OpenAI API.

Values:

- `true` -> use OpenAI
- `false` -> use mock fallback analyzer only

#### `OpenAI:ApiKey`

Your OpenAI API key.

Required when:

- `OpenAI:Enabled=true`

#### `OpenAI:Model`

The model used for complaint classification.

Recommended cost-effective default:

- `gpt-5.4-nano`

Higher-quality option:

- `gpt-5.4-mini`

#### `OpenAI:Endpoint`

The OpenAI Responses API endpoint.

Default:

- `https://api.openai.com/v1/responses`

#### `OpenAI:UseMockFallback`

If enabled, the app falls back to the local mock AI analyzer when OpenAI fails.

Values:

- `true` -> safe fallback behavior
- `false` -> throw error if OpenAI request fails

## 5. Environment Variable Overrides

ASP.NET Core supports environment variable overrides using double underscores.

### MongoDB Variables

- `MongoDb__ConnectionString`
- `MongoDb__DatabaseName`
- `MongoDb__ComplaintsCollectionName`
- `MongoDb__UsersCollectionName`

### OpenAI Variables

- `OpenAI__Enabled`
- `OpenAI__ApiKey`
- `OpenAI__Model`
- `OpenAI__Endpoint`
- `OpenAI__UseMockFallback`

### Startup/Test Variable

- `SeedData`

## 6. Example Environment Setup

### Local development without OpenAI

```bash
export MongoDb__ConnectionString="mongodb://localhost:27017"
export MongoDb__DatabaseName="IssueSenseDb"
export OpenAI__Enabled="false"
export SeedData="true"
```

### Local development with OpenAI enabled

```bash
export MongoDb__ConnectionString="mongodb://localhost:27017"
export MongoDb__DatabaseName="IssueSenseDb"
export OpenAI__Enabled="true"
export OpenAI__ApiKey="your_openai_api_key"
export OpenAI__Model="gpt-5.4-nano"
export OpenAI__UseMockFallback="true"
export SeedData="true"
```

### Test execution

For automated tests, disable startup seeding so the MVC test host can boot without depending on demo-data initialization:

```bash
export SeedData="false"
```

## 7. Launch Settings

Launch profile file:

- [launchSettings.json](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Properties/launchSettings.json)

Current development URLs are configured to fixed ports:

- `http://localhost:5000`
- `https://localhost:5001`

This avoids the Kestrel `localhost:0` dynamic port issue.

## 8. Application Startup Flow

Startup is configured in:

- [Program.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Program.cs)

At startup the app:

1. reads configuration
2. registers MongoDB context
3. registers repositories
4. registers services
5. configures cookie authentication
6. optionally seeds demo users
7. optionally seeds sample complaints

Seeding is controlled by:

- `SeedData=true` -> seed demo users and complaints
- `SeedData=false` -> skip startup seeding

## 9. Dependency Injection Registrations

Current registrations include:

- `MongoDbContext`
- `IUserRepository`
- `IComplaintRepository`
- `IUserService`
- `IComplaintService`
- `IAIAnalysisService`

OpenAI integration uses:

- `HttpClient`

## 10. Testing

Test project:

- [IssueSense.Tests.csproj](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Tests/IssueSense.Tests.csproj)

Current test setup includes:

- xUnit
- Moq
- `Microsoft.AspNetCore.Mvc.Testing`

Current test coverage includes:

- complaint service unit tests
- MVC integration tests for login and authentication redirects

Run tests with:

```bash
dotnet test IssueSense.slnx -p:UseSharedCompilation=false -maxcpucount:1 -nr:false
```

## 11. Internal Service API Flow

Even though this is not a public Web API app, the system exposes internal application service APIs through interfaces.

### `IUserService`

File:

- [IUserService.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Application/Interfaces/Services/IUserService.cs)

Responsibilities:

- login
- role checks
- user seeding

### `IComplaintService`

File:

- [IComplaintService.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Application/Interfaces/Services/IComplaintService.cs)

Responsibilities:

- create complaint
- query complaint list
- get details
- update status
- add comments
- update assignment
- manual re-analysis
- dashboard analytics
- sample complaint seeding

### `IAIAnalysisService`

File:

- [IAIAnalysisService.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Application/Interfaces/Services/IAIAnalysisService.cs)

Responsibilities:

- analyze complaint text
- return structured AI result

## 11. AI Request/Response Structure

The AI service returns a structured result containing:

- `sentiment`
- `category`
- `urgency`
- `confidence`
- `requiresAction`
- `suggestedActions`

Each action item includes:

- `owner`
- `action`

### Example Result

```json
{
  "sentiment": "negative",
  "category": "Billing",
  "urgency": "high",
  "confidence": 0.91,
  "requiresAction": true,
  "suggestedActions": [
    {
      "owner": "@triage_officer",
      "action": "Review this complaint immediately and confirm escalation priority."
    },
    {
      "owner": "@case_manager",
      "action": "Validate billing records and prepare a customer update or refund decision."
    }
  ]
}
```

## 12. Mock Fallback Behavior

If OpenAI is disabled or unavailable, the app uses a built-in analyzer in:

- [AIAnalysisService.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure/Services/AIAnalysisService.cs)

The fallback logic uses keyword heuristics to infer:

- sentiment
- category
- urgency
- confidence
- required action
- suggested actions with `@owner`

This means the app remains fully runnable without a real AI key.

## 13. Seeding and AI Interaction

During startup:

- demo users are seeded
- about 100 sample complaints are seeded

Complaint seeding also triggers AI analysis so the seeded data includes:

- sentiment
- urgency
- escalation
- assignment suggestion
- AI action items

## 14. Demo Accounts

Default users:

- `admin / Admin@123`
- `analyst / Analyst@123`
- `triage / Triage@123`
- `casemanager / Case@123`
- `aireviewer / Review@123`

## 15. How to Run

From the project root:

```bash
dotnet build IssueSense.slnx
dotnet run --project IssueSense.Web
```

Open:

- [http://localhost:5000](http://localhost:5000)
- or [https://localhost:5001](https://localhost:5001)

## 16. Typical Setup Scenarios

### Scenario A: Demo mode

Use:

- MongoDB running locally
- OpenAI disabled

Good for:

- local demo
- development
- offline testing

### Scenario B: Real AI mode

Use:

- MongoDB running locally or in cloud
- OpenAI enabled
- valid API key

Good for:

- real AI complaint classification
- richer action suggestions
- realistic confidence output

### Scenario C: Safe hybrid mode

Use:

- OpenAI enabled
- `UseMockFallback=true`

Good for:

- development environments
- unstable network scenarios
- cost-controlled testing

## 17. Troubleshooting

### MongoDB timeout / connection refused

Cause:

- MongoDB not running
- wrong connection string

Fix:

- start MongoDB
- verify `MongoDb:ConnectionString`

### App fails to start with localhost dynamic binding

Cause:

- invalid `launchSettings.json` URL like `localhost:0`

Fix:

- use fixed ports such as `5000` and `5001`

### OpenAI key missing

Cause:

- `OpenAI:Enabled=true` but no API key set

Fix:

- set `OpenAI__ApiKey`
- or disable OpenAI

### OpenAI request fails

If `UseMockFallback=true`, the app falls back automatically.

If `UseMockFallback=false`, the request throws an error and must be fixed directly.

## 18. Recommended Production Defaults

Suggested production-oriented settings:

- `OpenAI__Enabled=true`
- `OpenAI__Model=gpt-5.4-nano`
- `OpenAI__UseMockFallback=true`
- secure MongoDB connection string
- environment-variable-based secret management instead of plain appsettings

## 19. Related Files

- [Program.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Program.cs)
- [appsettings.json](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/appsettings.json)
- [launchSettings.json](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Web/Properties/launchSettings.json)
- [MongoDbContext.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure/Contexts/MongoDbContext.cs)
- [OpenAISettings.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure/Configuration/OpenAISettings.cs)
- [AIAnalysisService.cs](/Users/chason/Documents/GitHub/IssueSense/IssueSense.Infrastructure/Services/AIAnalysisService.cs)
- [README.md](/Users/chason/Documents/GitHub/IssueSense/README.md)
