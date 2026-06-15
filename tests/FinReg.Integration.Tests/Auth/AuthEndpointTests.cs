using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinReg.Integration.Tests.Infrastructure;
using FluentAssertions;

namespace FinReg.Integration.Tests.Auth;

public sealed class AuthEndpointTests(FinRegWebAppFactory factory) : IClassFixture<FinRegWebAppFactory>
{
    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithAccessToken()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = TestCredentials.Email,
            Password = TestCredentials.Password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("data").GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = TestCredentials.Email,
            Password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "nobody@finreg.test",
            Password = "DoesNotMatter!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
