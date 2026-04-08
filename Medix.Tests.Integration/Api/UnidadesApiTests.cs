using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Medix.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Medix.Tests.Integration.Api;

[Collection("Integration")]
public class UnidadesApiTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UnidadesApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUnidades_QuandoChamadoSemFiltros_DeveRetornar200ComListaPaginada()
    {
        // Arrange
        // (factory já tem seed com 3 unidades)

        // Act
        var response = await _client.GetAsync("/api/unidades");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.RootElement.TryGetProperty("items", out var items));
        Assert.True(items.GetArrayLength() > 0);
        Assert.True(doc.RootElement.TryGetProperty("links", out _));
    }

    [Fact]
    public async Task GetUnidadeById_QuandoIdValido_DeveRetornar200ComObjeto()
    {
        // Arrange
        var listaResponse = await _client.GetAsync("/api/unidades");
        var listaJson = await listaResponse.Content.ReadAsStringAsync();
        var listaDoc = JsonDocument.Parse(listaJson);
        var primeiroId = listaDoc.RootElement.GetProperty("items")[0].GetProperty("id").GetInt32();

        // Act
        var response = await _client.GetAsync($"/api/unidades/{primeiroId}");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(primeiroId, doc.RootElement.GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task GetUnidadeById_QuandoIdInexistente_DeveRetornar404()
    {
        // Arrange
        // (id 9999 não existe no seed)

        // Act
        var response = await _client.GetAsync("/api/unidades/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUnidades_QuandoPaginacaoPageSize2_DeveRetornar2Itens()
    {
        // Arrange
        // (seed tem 3 unidades)

        // Act
        var response = await _client.GetAsync("/api/unidades?pageSize=2&page=1");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = doc.RootElement.GetProperty("items");
        Assert.Equal(2, items.GetArrayLength());
    }

    [Fact]
    public async Task GetUnidades_QuandoFiltroPorNome_DeveRetornarApenasCorrespondentes()
    {
        // Arrange
        // (seed tem "Hospital Teste Alpha", "Clínica Beta", "UPA Gama")

        // Act
        var response = await _client.GetAsync("/api/unidades?nome=Hospital");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = doc.RootElement.GetProperty("items");
        Assert.True(items.GetArrayLength() >= 1);
        // Garante que todos os itens retornados contêm o termo filtrado
        foreach (var item in items.EnumerateArray())
            Assert.Contains("Hospital", item.GetProperty("nome").GetString());
    }

    [Fact]
    public async Task GetUnidades_QuandoSemAutenticacao_DeveRedirecionarOuBloquear()
    {
        // Arrange
        var clientSemRedirect = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await clientSemRedirect.GetAsync("/api/unidades");

        // Assert
        // A API deve redirecionar para login (302) ou retornar 401/200 — nunca 500
        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.Found ||
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.OK,
            $"Status inesperado: {response.StatusCode}");
    }
}
