# IssueSense

ASP.NET Core MVC application for AI-based complaint and sentiment analysis using MongoDB.

## Default Users

- `admin` / `Admin@123` (`support_admin`)
- `analyst` / `Analyst@123` (`analyst`)

## Architecture

- `IssueSense.Web`: MVC UI, controllers, Razor views, authentication setup
- `IssueSense.Application`: DTOs, service interfaces, and business logic
- `IssueSense.Domain`: Core entities and enums
- `IssueSense.Infrastructure`: MongoDB context, document models, repositories, and sentiment analysis implementation
- `IssueSense.Tests`: xUnit test project for service and MVC integration tests

## Run

1. Start MongoDB locally on `mongodb://localhost:27017`
2. If you want demo seed data locally, set `SeedData=true`
3. Run `dotnet build`
4. Run `dotnet run --project IssueSense.Web`

Notes:

- `SeedData` now defaults to `false` for safer production behavior
- in production or on Render, keep `SeedData=false`
- the app now configures forwarded headers and secure cookies so it behaves correctly behind an HTTPS reverse proxy
- MongoDB indexes are created automatically on startup, including a unique username index

## Test

Run the test suite with:

```bash
dotnet test IssueSense.slnx -p:UseSharedCompilation=false -maxcpucount:1 -nr:false
```

Current coverage includes:

- `ComplaintService` unit tests for complaint creation and manual re-analysis
- MVC integration tests for the login page and anonymous redirect behavior

## OpenAI Integration

The app can use OpenAI for complaint analysis through the Responses API.

Configure these settings in `IssueSense.Web/appsettings.json` or environment variables:

- `OpenAI__Enabled=true`
- `OpenAI__ApiKey=your_api_key`
- `OpenAI__Model=gpt-5.4-nano`
- `OpenAI__Endpoint=https://api.openai.com/v1/responses`

If OpenAI is disabled or the API call fails, the app falls back to the built-in mock classifier when `OpenAI__UseMockFallback=true`.

Keep secrets out of source control and prefer environment variables for `OpenAI__ApiKey`.

## Cloud Run Deployment

This repo includes:

- [Dockerfile](/Users/chason/Documents/GitHub/IssueSense/Dockerfile)
- [.dockerignore](/Users/chason/Documents/GitHub/IssueSense/.dockerignore)

Typical Google Cloud Run deployment flow:

```bash
gcloud config set project YOUR_PROJECT_ID

gcloud run deploy issuesense \
  --source . \
  --region us-central1 \
  --allow-unauthenticated \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production,SeedData=false,OpenAI__Enabled=false,MongoDb__DatabaseName=IssueSenseDb,MongoDb__ComplaintsCollectionName=complaints,MongoDb__UsersCollectionName=users \
  --set-secrets OpenAI__ApiKey=OPENAI_API_KEY:latest
```

Also set your MongoDB connection string in Cloud Run:

```bash
gcloud run services update issuesense \
  --region us-central1 \
  --update-env-vars MongoDb__ConnectionString='YOUR_MONGODB_CONNECTION_STRING'
```

Recommended production values:

- `ASPNETCORE_ENVIRONMENT=Production`
- `SeedData=false`
- `OpenAI__Enabled=true` or `false`
- `OpenAI__Model=gpt-5.4-nano`
- `OpenAI__UseMockFallback=true`

After deployment, verify:

- `/health`
- login page loads
- MongoDB connection works
- complaint list page loads
