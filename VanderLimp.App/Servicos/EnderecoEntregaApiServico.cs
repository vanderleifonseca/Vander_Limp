using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VanderLimp.App.Dados;

namespace VanderLimp.App.Servicos;

public class EnderecoEntregaApiServico
{
    private readonly HttpClient _httpClient;
    private readonly SessaoLocalServico _sessaoLocal;

    public EnderecoEntregaApiServico(
        HttpClient httpClient,
        SessaoLocalServico sessaoLocal)
    {
        _httpClient = httpClient;
        _sessaoLocal = sessaoLocal;
    }

    public async Task<List<EnderecoEntregaDados>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        string? token = await _sessaoLocal.ObterTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Entre novamente para consultar seus endereços.");
        }

        using var requisicao = new HttpRequestMessage(
            HttpMethod.Get,
            "api/enderecos-entrega");

        requisicao.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var resposta = await _httpClient.SendAsync(
            requisicao,
            cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.Unauthorized)
        {
            _sessaoLocal.EncerrarSessaoLocal();

            throw new UnauthorizedAccessException(
                "Sua sessão terminou. Entre novamente.");
        }

        if (!resposta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "Não foi possível consultar os endereços de entrega.");
        }

        var enderecos = await resposta.Content
            .ReadFromJsonAsync<List<EnderecoEntregaDados>>(
                cancellationToken: cancellationToken);

        return enderecos ?? throw new InvalidOperationException(
            "A API retornou uma lista de endereços inválida.");
    }
}