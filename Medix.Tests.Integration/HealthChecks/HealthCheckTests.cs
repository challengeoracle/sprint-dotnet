using System.Net;
using System.Text.Json;
using Medix.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Medix.Tests.Integration.HealthChecks;

[Collection("Integration")]
public class HealthCheckTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_QuandoChamado_DeveRetornar200()
    {
        // Arrange
        // (nenhuma pré-condição necessária)

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_QuandoChamado_DeveRetornarJsonComStatus()
    {
        // Arrange
        // (nenhuma pré-condição necessária)

        // Act
        var response = await _client.GetAsync("/health");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.True(doc.RootElement.TryGetProperty("status", out var status));
        Assert.Equal("Healthy", status.GetString());
    }

    [Fact]
    public async Task HealthReadyEndpoint_QuandoChamado_DeveRetornar200()
    {
        // Arrange
        // (nenhuma pré-condição necessária)

        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthLiveEndpoint_QuandoChamado_DeveRetornar200EStatusHealthy()
    {
        // Arrange
        // (nenhuma pré-condição necessária)

        // Act
        var response = await _client.GetAsync("/health/live");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task DashboardMvc_QuandoSemLogin_DeveRedirecionarParaLogin()
    {
        // Arrange
        var clientSemRedirect = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        // Header especial que instrui o TestAuthHandler a simular usuário não autenticado
        clientSemRedirect.DefaultRequestHeaders.Add(TestAuthHandler.SkipAuthHeader, "true");

        // Act
        var response = await clientSemRedirect.GetAsync("/UnidadeSaude/Dashboard");

        // Assert
        // Deve bloquear o acesso: 302 redirect (Identity cookie) ou 401 Unauthorized (outros schemes)
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Esperado 302 ou 401, mas recebeu {response.StatusCode}");
    }
}
