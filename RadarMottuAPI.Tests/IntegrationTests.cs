using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RadarMottuAPI.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public IntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Health_Should_Return_200()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health");
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Anchors_Without_JWT_Should_401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/v1/anchors");
        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anchors_With_JWT_Should_200()
    {
        var client = _factory.CreateClient();

        var loginBody = JsonSerializer.Serialize(new { usuario = "jp", senha = "123" });
        var tokenResp = await client.PostAsync("/api/v1/auth/login",
            new StringContent(loginBody, Encoding.UTF8, "application/json"));
        tokenResp.EnsureSuccessStatusCode();
        var token = (await tokenResp.Content.ReadAsStringAsync()).Replace("\"", "");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/v1/anchors");
        resp.EnsureSuccessStatusCode();
    }
}
