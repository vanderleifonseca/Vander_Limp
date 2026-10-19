using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VanderLimp.Admin.Dados;

namespace VanderLimp.Admin.Servicos;

public class ClienteAdminApiServico
{
    private readonly HttpClient _httpClient;

    public ClienteAdminApiServico(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ClienteAdminDados>> ListarAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Entre novamente no painel administrativo.");
        }

        using var requisicao = new HttpRequestMessage(
            HttpMethod.Get,
            "api/admin/clientes");

        requisicao.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var resposta = await _httpClient.SendAsync(
            requisicao,
            cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.Unauthorized ||
            resposta.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "Sua sessão terminou ou não permite consultar clientes.");
        }

        if (!resposta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "Não foi possível consultar os clientes. Tente novamente.");
        }

        var clientes = await resposta.Content
            .ReadFromJsonAsync<List<ClienteAdminDados>>(
                cancellationToken: cancellationToken);

        return clientes ?? throw new InvalidOperationException(
            "A API retornou uma lista de clientes inválida.");
    }
    public async Task<List<EnderecoAdminDados>> ListarEnderecosAsync(
    string token,
    int clienteId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Entre novamente no painel administrativo.");
        }

        if (clienteId <= 0)
        {
            throw new InvalidOperationException(
                "Selecione um cliente válido.");
        }

        using var requisicao = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/admin/clientes/{clienteId}/enderecos");

        requisicao.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var resposta = await _httpClient.SendAsync(
            requisicao,
            cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.Unauthorized ||
            resposta.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "Sua sessão terminou ou não permite consultar endereços.");
        }

        if (!resposta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "Não foi possível consultar os endereços. Tente novamente.");
        }

        var enderecos = await resposta.Content
            .ReadFromJsonAsync<List<EnderecoAdminDados>>(
                cancellationToken: cancellationToken);

        return enderecos ?? throw new InvalidOperationException(
            "A API retornou uma lista de endereços inválida.");
    }
}