using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using VanderLimp.Admin.Dados;

namespace VanderLimp.Admin.Servicos;

public class ProdutoAdminApiServico
{
    private readonly HttpClient _httpClient;

    public ProdutoAdminApiServico(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProdutoAdminDados>> ListarAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        using var requisicao = CriarRequisicao(
            HttpMethod.Get,
            "api/admin/produtos",
            token);

        using var resposta = await _httpClient.SendAsync(
            requisicao,
            cancellationToken);

        await ConferirRespostaAsync(resposta, cancellationToken);

        var produtos = await resposta.Content
            .ReadFromJsonAsync<List<ProdutoAdminDados>>(
                cancellationToken: cancellationToken);

        return produtos ?? throw new InvalidOperationException(
            "A API retornou uma lista inválida.");
    }

    public async Task CadastrarAsync(
        string token,
        ProdutoCadastroDados dados,
        CancellationToken cancellationToken = default)
    {
        using var requisicao = CriarRequisicao(
            HttpMethod.Post,
            "api/admin/produtos",
            token);

        requisicao.Content = JsonContent.Create(dados);

        using var resposta = await _httpClient.SendAsync(
            requisicao,
            cancellationToken);

        await ConferirRespostaAsync(resposta, cancellationToken);
    }

    public async Task AtualizarAsync(
        string token,
        int id,
        ProdutoCadastroDados dados,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new InvalidOperationException(
                "Selecione um produto válido para editar.");
        }

        using var requisicao = CriarRequisicao(
            HttpMethod.Put,
            $"api/admin/produtos/{id}",
            token);

        requisicao.Content = JsonContent.Create(dados);

        using var resposta = await _httpClient.SendAsync(
            requisicao,
            cancellationToken);

        await ConferirRespostaAsync(resposta, cancellationToken);
    }

    private static HttpRequestMessage CriarRequisicao(
        HttpMethod metodo,
        string endereco,
        string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new UnauthorizedAccessException(
                "Entre novamente no painel administrativo.");
        }

        var requisicao = new HttpRequestMessage(metodo, endereco);

        requisicao.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return requisicao;
    }

    private static async Task ConferirRespostaAsync(
        HttpResponseMessage resposta,
        CancellationToken cancellationToken)
    {
        if (resposta.StatusCode == HttpStatusCode.Unauthorized ||
            resposta.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "Sua sessão não permite esta operação. Entre novamente.");
        }

        if (resposta.IsSuccessStatusCode)
            return;

        string mensagem =
            "Não foi possível concluir a operação com os produtos.";

        string conteudo = await resposta.Content.ReadAsStringAsync(
            cancellationToken);

        try
        {
            using var documento = JsonDocument.Parse(conteudo);
            var raiz = documento.RootElement;

            if (raiz.ValueKind == JsonValueKind.Object)
            {
                if (raiz.TryGetProperty("errors", out var erros) &&
                    erros.ValueKind == JsonValueKind.Object)
                {
                    var mensagens = new List<string>();

                    foreach (var campo in erros.EnumerateObject())
                    {
                        if (campo.Value.ValueKind != JsonValueKind.Array)
                            continue;

                        foreach (var erro in campo.Value.EnumerateArray())
                        {
                            if (erro.ValueKind == JsonValueKind.String &&
                                !string.IsNullOrWhiteSpace(erro.GetString()))
                            {
                                mensagens.Add(erro.GetString()!);
                            }
                        }
                    }

                    if (mensagens.Count > 0)
                    {
                        mensagem = string.Join(
                            " ",
                            mensagens.Distinct());
                    }
                }
                else if (raiz.TryGetProperty("mensagem", out var texto) &&
                         texto.ValueKind == JsonValueKind.String)
                {
                    mensagem = texto.GetString() ?? mensagem;
                }
                else if (raiz.TryGetProperty("title", out var titulo) &&
                         titulo.ValueKind == JsonValueKind.String)
                {
                    mensagem = titulo.GetString() ?? mensagem;
                }
            }
        }
        catch (JsonException)
        {
            // Mantém a mensagem padrão se a resposta não for JSON.
        }

        throw new InvalidOperationException(mensagem);
    }
}