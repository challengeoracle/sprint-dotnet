using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Medix.Tests.Integration.Fixtures;

namespace Medix.Tests.Integration.Api;

[Collection("Integration")]
public class PacientesApiTests
{
    private readonly HttpClient _client;

    // O seed tem unidades com IDs 1, 2, 3 — usamos a 1 como base
    private const string BaseUrl = "/api/unidades/1/pacientes";

    public PacientesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPacientes_QuandoUnidadeExistente_DeveRetornar200()
    {
        // Arrange
        // (unidade 1 existe no seed)

        // Act
        var response = await _client.GetAsync(BaseUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPacientes_QuandoUnidadeInexistente_DeveRetornar404()
    {
        // Arrange
        // (unidade 99999 não existe)

        // Act
        var response = await _client.GetAsync("/api/unidades/99999/pacientes");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPacientes_QuandoChamado_DeveRetornarEstruturaPaginada()
    {
        // Arrange
        // (nenhuma pré-condição necessária)

        // Act
        var response = await _client.GetAsync(BaseUrl);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(doc.RootElement.TryGetProperty("items", out _));
        Assert.True(doc.RootElement.TryGetProperty("totalCount", out _));
        Assert.True(doc.RootElement.TryGetProperty("links", out _));
    }

    // ── POST ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PostPaciente_QuandoDadosValidos_DeveRetornar201()
    {
        // Arrange
        var payload = new
        {
            nomeCompleto = "João da Silva",
            cpf = "123.456.789-00",
            email = "joao@teste.com",
            telefone = "(11) 91234-5678"
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostPaciente_QuandoDadosValidos_DeveRetornarLocationHeader()
    {
        // Arrange
        var payload = new
        {
            nomeCompleto = "Maria Location",
            email = "maria.location@teste.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/pacientes/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task PostPaciente_QuandoDadosValidos_DeveRetornarObjetoComLinks()
    {
        // Arrange
        var payload = new
        {
            nomeCompleto = "Pedro Links",
            email = "pedro.links@teste.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Pedro Links", doc.RootElement.GetProperty("nomeCompleto").GetString());
        Assert.Equal(1, doc.RootElement.GetProperty("unidadeMedicaId").GetInt32());
        Assert.True(doc.RootElement.TryGetProperty("links", out var links));
        Assert.True(links.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PostPaciente_QuandoNomeFaltando_DeveRetornar400()
    {
        // Arrange
        var payload = new
        {
            // nomeCompleto ausente
            email = "sem-nome@teste.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostPaciente_QuandoUnidadeInexistente_DeveRetornar404()
    {
        // Arrange
        var payload = new
        {
            nomeCompleto = "Paciente Sem Unidade"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/unidades/99999/pacientes", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── PUT ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PutPaciente_QuandoIdExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange — cria paciente para editar
        var criarPayload = new { nomeCompleto = "Paciente Para Editar", email = "p.editar@teste.com" };
        var criarResponse = await _client.PostAsJsonAsync(BaseUrl, criarPayload);
        var id = JsonDocument.Parse(await criarResponse.Content.ReadAsStringAsync())
                             .RootElement.GetProperty("id").GetInt32();

        var updatePayload = new { nomeCompleto = "Paciente Editado", email = "p.editado@teste.com" };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", updatePayload);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Paciente Editado", doc.RootElement.GetProperty("nomeCompleto").GetString());
    }

    [Fact]
    public async Task PutPaciente_QuandoIdInexistente_DeveRetornar404()
    {
        // Arrange
        var payload = new { nomeCompleto = "Qualquer Nome" };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/99999", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePaciente_QuandoIdExistente_DeveRetornar204()
    {
        // Arrange
        var criarPayload = new { nomeCompleto = "Paciente Para Deletar" };
        var criarResponse = await _client.PostAsJsonAsync(BaseUrl, criarPayload);
        var id = JsonDocument.Parse(await criarResponse.Content.ReadAsStringAsync())
                             .RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeletePaciente_QuandoIdInexistente_DeveRetornar404()
    {
        // Arrange
        // (id 99999 não existe)

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeletePaciente_QuandoDeletado_NaoDeveEncontrarNovamente()
    {
        // Arrange
        var criarPayload = new { nomeCompleto = "Paciente Deletado Verificado" };
        var criarResponse = await _client.PostAsJsonAsync(BaseUrl, criarPayload);
        var id = JsonDocument.Parse(await criarResponse.Content.ReadAsStringAsync())
                             .RootElement.GetProperty("id").GetInt32();

        // Act
        await _client.DeleteAsync($"{BaseUrl}/{id}");
        var getResponse = await _client.GetAsync($"{BaseUrl}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
