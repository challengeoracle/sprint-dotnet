using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Medix.Tests.Integration.Fixtures;

namespace Medix.Tests.Integration.Api;

[Collection("Integration")]
public class UnidadesApiMutationTests
{
    private readonly HttpClient _client;

    public UnidadesApiMutationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── POST ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PostUnidade_QuandoDadosValidos_DeveRetornar201()
    {
        // Arrange
        var payload = new
        {
            nome = "Hospital Novo Teste",
            cnpj = "44.444.444/0001-44",
            emailAdmin = "novo@teste.com",
            status = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/unidades", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUnidade_QuandoDadosValidos_DeveRetornarLocationHeader()
    {
        // Arrange
        var payload = new
        {
            nome = "Hospital Location Test",
            cnpj = "55.555.555/0001-55",
            emailAdmin = "location@teste.com",
            status = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/unidades", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/api/unidades/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task PostUnidade_QuandoDadosValidos_DeveRetornarObjetoComLinks()
    {
        // Arrange
        var payload = new
        {
            nome = "Hospital Links Test",
            cnpj = "66.666.666/0001-66",
            emailAdmin = "links@teste.com",
            status = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/unidades", payload);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Hospital Links Test", doc.RootElement.GetProperty("nome").GetString());
        Assert.True(doc.RootElement.TryGetProperty("links", out var links));
        Assert.True(links.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PostUnidade_QuandoNomeFaltando_DeveRetornar400()
    {
        // Arrange
        var payload = new
        {
            // nome ausente
            cnpj = "77.777.777/0001-77",
            emailAdmin = "sem-nome@teste.com",
            status = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/unidades", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostUnidade_QuandoEmailInvalido_DeveRetornar400()
    {
        // Arrange
        var payload = new
        {
            nome = "Hospital Email Inválido",
            cnpj = "88.888.888/0001-88",
            emailAdmin = "nao-e-um-email",
            status = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/unidades", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── PUT ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PutUnidade_QuandoIdExistente_DeveRetornar200()
    {
        // Arrange — cria uma unidade para editar sem tocar no seed
        var criarPayload = new
        {
            nome = "Hospital Para Editar",
            cnpj = "91.111.111/0001-91",
            emailAdmin = "editar@teste.com",
            status = 0
        };
        var criarResponse = await _client.PostAsJsonAsync("/api/unidades", criarPayload);
        var criarJson = await criarResponse.Content.ReadAsStringAsync();
        var id = JsonDocument.Parse(criarJson).RootElement.GetProperty("id").GetInt32();

        var updatePayload = new
        {
            nome = "Hospital Editado",
            cnpj = "91.111.111/0001-91",
            emailAdmin = "editado@teste.com",
            status = 1
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/unidades/{id}", updatePayload);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Hospital Editado", doc.RootElement.GetProperty("nome").GetString());
    }

    [Fact]
    public async Task PutUnidade_QuandoIdInexistente_DeveRetornar404()
    {
        // Arrange
        var payload = new
        {
            nome = "Qualquer Nome",
            cnpj = "00.000.000/0001-00",
            emailAdmin = "qualquer@teste.com",
            status = 0
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/unidades/99999", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutUnidade_QuandoDadosValidos_DeveRetornarObjetoAtualizado()
    {
        // Arrange
        var criarPayload = new
        {
            nome = "Hospital Para Atualizar",
            cnpj = "92.222.222/0001-92",
            emailAdmin = "atualizar@teste.com",
            status = 0
        };
        var criarResponse = await _client.PostAsJsonAsync("/api/unidades", criarPayload);
        var id = JsonDocument.Parse(await criarResponse.Content.ReadAsStringAsync())
                             .RootElement.GetProperty("id").GetInt32();

        var updatePayload = new
        {
            nome = "Hospital Atualizado",
            cnpj = "92.222.222/0001-92",
            emailAdmin = "atualizado@teste.com",
            status = 2
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/unidades/{id}", updatePayload);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(id, doc.RootElement.GetProperty("id").GetInt32());
        Assert.Equal("Hospital Atualizado", doc.RootElement.GetProperty("nome").GetString());
        Assert.True(doc.RootElement.TryGetProperty("links", out _));
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUnidade_QuandoIdExistente_DeveRetornar204()
    {
        // Arrange — cria uma unidade específica para deletar
        var criarPayload = new
        {
            nome = "Hospital Para Deletar",
            cnpj = "93.333.333/0001-93",
            emailAdmin = "deletar@teste.com",
            status = 0
        };
        var criarResponse = await _client.PostAsJsonAsync("/api/unidades", criarPayload);
        var id = JsonDocument.Parse(await criarResponse.Content.ReadAsStringAsync())
                             .RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await _client.DeleteAsync($"/api/unidades/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUnidade_QuandoIdInexistente_DeveRetornar404()
    {
        // Arrange
        // (id 99999 não existe)

        // Act
        var response = await _client.DeleteAsync("/api/unidades/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUnidade_QuandoDeletado_NaoDeveEncontrarNovamente()
    {
        // Arrange
        var criarPayload = new
        {
            nome = "Hospital Deletado Verificado",
            cnpj = "94.444.444/0001-94",
            emailAdmin = "deletado@teste.com",
            status = 0
        };
        var criarResponse = await _client.PostAsJsonAsync("/api/unidades", criarPayload);
        var id = JsonDocument.Parse(await criarResponse.Content.ReadAsStringAsync())
                             .RootElement.GetProperty("id").GetInt32();

        // Act
        await _client.DeleteAsync($"/api/unidades/{id}");
        var getResponse = await _client.GetAsync($"/api/unidades/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
