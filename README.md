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
2. Run `dotnet build`
3. Run `dotnet run --project IssueSense.Web`

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
