using IssueSense.Application.Interfaces.Repositories;
using IssueSense.Application.Interfaces.Services;
using IssueSense.Application.Services;
using IssueSense.Infrastructure.Configuration;
using IssueSense.Infrastructure.Contexts;
using IssueSense.Infrastructure.Repositories;
using IssueSense.Infrastructure.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var mongoDbSettings = builder.Configuration
    .GetSection(MongoDbSettings.SectionName)
    .Get<MongoDbSettings>() ?? new MongoDbSettings();
var openAiSettings = builder.Configuration
    .GetSection(OpenAISettings.SectionName)
    .Get<OpenAISettings>() ?? new OpenAISettings();

builder.Services.AddSingleton(mongoDbSettings);
builder.Services.AddSingleton(openAiSettings);
builder.Services.AddScoped<MongoDbContext>();

builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpClient<IAIAnalysisService, AIAnalysisService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.Logger.LogInformation(
    "Startup configuration resolved. Environment: {Environment}. SeedData: {SeedData}. MongoDb database: {DatabaseName}. MongoDb connection: {ConnectionString}",
    app.Environment.EnvironmentName,
    builder.Configuration.GetValue("SeedData", false),
    mongoDbSettings.DatabaseName,
    mongoDbSettings.ConnectionString);

using (var scope = app.Services.CreateScope())
{
    var mongoDbContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    app.Logger.LogInformation("Ensuring MongoDB indexes for database {DatabaseName}.", mongoDbSettings.DatabaseName);
    await mongoDbContext.EnsureIndexesAsync();
    app.Logger.LogInformation("MongoDB indexes ensured successfully.");
}

var shouldSeedData = app.Environment.IsDevelopment() && builder.Configuration.GetValue("SeedData", false);

app.Logger.LogInformation(
    "Seed evaluation complete. IsDevelopment: {IsDevelopment}. ShouldSeedData: {ShouldSeedData}.",
    app.Environment.IsDevelopment(),
    shouldSeedData);

if (shouldSeedData)
{
    using var scope = app.Services.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    var complaintService = scope.ServiceProvider.GetRequiredService<IComplaintService>();
    var complaintRepository = scope.ServiceProvider.GetRequiredService<IComplaintRepository>();
    var complaintsBefore = await complaintRepository.GetAllAsync();
    app.Logger.LogInformation(
        "Pre-seed complaint count: {TotalCount}. Active: {ActiveCount}. Archived: {ArchivedCount}.",
        complaintsBefore.Count,
        complaintsBefore.Count(x => !x.IsArchived),
        complaintsBefore.Count(x => x.IsArchived));
    app.Logger.LogInformation("Starting default user and complaint seeding.");
    await userService.SeedDefaultUsersAsync();
    await complaintService.SeedSampleComplaintsAsync(100);
    var complaintsAfter = await complaintRepository.GetAllAsync();
    app.Logger.LogInformation(
        "Seeding completed. Post-seed complaint count: {TotalCount}. Active: {ActiveCount}. Archived: {ArchivedCount}.",
        complaintsAfter.Count,
        complaintsAfter.Count(x => !x.IsArchived),
        complaintsAfter.Count(x => x.IsArchived));
}
else
{
    app.Logger.LogInformation("Seeding skipped.");
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("An unexpected error occurred.");
        });
    });
    app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    HttpOnly = HttpOnlyPolicy.Always,
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = CookieSecurePolicy.Always
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

public partial class Program
{
}
