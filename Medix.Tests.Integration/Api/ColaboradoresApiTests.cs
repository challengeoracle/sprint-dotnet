using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Medix.Tests.Integration.Fixtures;

namespace Medix.Tests.Integration.Api;

[Collection("Integration")]
public class ColaboradoresApiTests
{
    private readonly HttpClient _client;

    // O seed tem unidades com IDs 1, 2, 3 — usamos a 2 para isolar do PacientesApiTests
    private const string BaseUrl = "/api/unidades/2/colaboradores";

    public ColaboradoresApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetColaboradores_QuandoUnidadeExistente_DeveRetornar200()
    {
        // Arrange
        // (unidade 2 existe no seed)

        // Act
        var response = await _client.GetAsync(BaseUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetColaboradores_QuandoUnidadeInexistente_DeveRetornar404()
    {
        // Arrange
        // (unidade 99999 não existe)

        // Act
        var response = await _client.GetAsync("/api/unidades/99999/colaboradores");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetColaboradores_QuandoChamado_DeveRetornarEstruturaPaginada()
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
    public async Task PostColaborador_QuandoDadosValidos_DeveRetornar201()
    {
        // Arrange
        var payload = new
        {
            nomeCompleto = "Dr. Carlos Médico",
            email = "carlos@teste.com",
            cargo = 0, // Medico
            especialidade = "Cardiologia",
            registroProfissional = "CRM-12345"
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostColaborador_QuandoDadosValidos_DeveRetornarLocationHeader()
    {
        // Arrange
        var payload = new
        {
            nomeCompleto = "Enf. Ana Location",
            cargo = 1 // Enfermeiro
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/colaboradores/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task PostColaborador_QuandoDadosValidos_DeveRetornarObjetoComCargoELinks()
    {
        // Arrange
        var payload = new
        {
            nomeCompleto = "Dr. Links Teste",
            email = "links.teste@hospital.com",
            cargo = 0, // Medico
            especialidade = "Ortopedia"
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Dr. Links Teste", doc.RootElement.GetProperty("nomeCompleto").GetString());
        Assert.Equal(2, doc.RootElement.GetProperty("unidadeMedicaId").GetInt32());
        Assert.True(doc.RootElement.TryGetProperty("links", out var links));
        Assert.True(links.GetArrayLength() > 0);
    }

    [Fact]
    public async Task PostColaborador_QuandoNomeFaltando_DeveRetornar400()
    {
        // Arrange
        var payload = new
        {
            // nomeCompleto ausente
            cargo = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync(BaseUrl, payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostColaborador_QuandoUnidadeInexistente_DeveRetornar404()
    {
        // Arrange
        var payload = new
        {
            nomeCompleto = "Colaborador Sem Unidade",
            cargo = 2 // Administrativo
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/unidades/99999/colaboradores", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── PUT ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PutColaborador_QuandoIdExistente_DeveRetornar200ComDadosAtualizados()
    {
        // Arrange
        var criarPayload = new { nomeCompleto = "Colaborador Para Editar", cargo = 2 };
        var criarResponse = await _client.PostAsJsonAsync(BaseUrl, criarPayload);
        var id = JsonDocument.Parse(await criarResponse.Content.ReadAsStringAsync())
                             .RootElement.GetProperty("id").GetInt32();

        var updatePayload = new { nomeCompleto = "Colaborador Editado", cargo = 0, especialidade = "Neurologia" };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/{id}", updatePayload);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Colaborador Editado", doc.RootElement.GetProperty("nomeCompleto").GetString());
    }

    [Fact]
    public async Task PutColaborador_QuandoIdInexistente_DeveRetornar404()
    {
        // Arrange
        var payload = new { nomeCompleto = "Qualquer Nome", cargo = 0 };

        // Act
        var response = await _client.PutAsJsonAsync($"{BaseUrl}/99999", payload);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteColaborador_QuandoIdExistente_DeveRetornar204()
    {
        // Arrange
        var criarPayload = new { nomeCompleto = "Colaborador Para Deletar", cargo = 1 };
        var criarResponse = await _client.PostAsJsonAsync(BaseUrl, criarPayload);
        var id = JsonDocument.Parse(await criarResponse.Content.ReadAsStringAsync())
                             .RootElement.GetProperty("id").GetInt32();

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteColaborador_QuandoIdInexistente_DeveRetornar404()
    {
        // Arrange
        // (id 99999 não existe)

        // Act
        var response = await _client.DeleteAsync($"{BaseUrl}/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteColaborador_QuandoDeletado_NaoDeveEncontrarNovamente()
    {
        // Arrange
        var criarPayload = new { nomeCompleto = "Colaborador Deletado Verificado", cargo = 2 };
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
