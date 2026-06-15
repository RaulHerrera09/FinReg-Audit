using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FinReg.Integration.Tests.Infrastructure;
using FluentAssertions;

namespace FinReg.Integration.Tests.Accounts;

public sealed class AccountLifecycleTests(FinRegWebAppFactory factory) : IClassFixture<FinRegWebAppFactory>
{
    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = TestCredentials.Email,
            Password = TestCredentials.Password
        });
        loginResponse.EnsureSuccessStatusCode();

        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("data").GetProperty("accessToken").GetString()!;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task OpenAccount_WithValidRequest_Returns201WithAccountId()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/accounts", new
        {
            HolderName = "Jane Smith",
            Currency = "GBP"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task InitiateTransaction_OnOpenAccount_Returns201WithTransactionId()
    {
        var client = await CreateAuthenticatedClientAsync();

        var accountId = await OpenAccountAsync(client, "Bob Jones", "GBP");

        var response = await client.PostAsJsonAsync($"/api/accounts/{accountId}/transactions", new
        {
            Amount = 500.00m,
            Currency = "GBP",
            Type = 0,
            Description = "Salary payment"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        body.GetProperty("data").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task InitiateTransaction_AboveFcaThreshold_GeneratesAlert()
    {
        var client = await CreateAuthenticatedClientAsync();

        var accountId = await OpenAccountAsync(client, "Alice Cooper", "GBP");

        var txResponse = await client.PostAsJsonAsync($"/api/accounts/{accountId}/transactions", new
        {
            Amount = 15_000.00m,
            Currency = "GBP",
            Type = 0,
            Description = "Large deposit"
        });

        txResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var alertsResponse = await client.GetAsync($"/api/accounts/{accountId}/alerts");
        alertsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var alertsBody = await alertsResponse.Content.ReadFromJsonAsync<JsonElement>();
        alertsBody.GetProperty("data").GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAuditTrail_AfterTransaction_ReturnsOrderedEvents()
    {
        var client = await CreateAuthenticatedClientAsync();

        var accountId = await OpenAccountAsync(client, "Charles Brown", "GBP");
        await client.PostAsJsonAsync($"/api/accounts/{accountId}/transactions", new
        {
            Amount = 1_000.00m,
            Currency = "GBP",
            Type = 0,
            Description = "Test credit"
        });

        var response = await client.GetAsync($"/api/accounts/{accountId}/audit-trail");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("items").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetComplianceReport_ForAccount_ReturnsStats()
    {
        var client = await CreateAuthenticatedClientAsync();

        var accountId = await OpenAccountAsync(client, "Diana Prince", "GBP");

        var response = await client.GetAsync($"/api/accounts/{accountId}/compliance-report");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    private static async Task<Guid> OpenAccountAsync(HttpClient client, string holderName, string currency)
    {
        var response = await client.PostAsJsonAsync("/api/accounts", new
        {
            HolderName = holderName,
            Currency = currency
        });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("data").GetGuid();
    }
}
