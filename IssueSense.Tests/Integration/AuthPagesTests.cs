using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IssueSense.Tests.Integration;

public sealed class AuthPagesTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthPagesTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task LoginPage_LoadsSuccessfully()
    {
        var response = await _client.GetAsync("/Auth/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_RedirectsAnonymousUsersToLogin()
    {
        var response = await _client.GetAsync("/Dashboard");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal("/Auth/Login", response.Headers.Location!.AbsolutePath);
    }
}
