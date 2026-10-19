using System.Net.Http.Json;
using System.Text.Json;
using VanderLimp.App.Dados;

namespace VanderLimp.App.Servicos;

public class ClienteApiServico
{
    private readonly HttpClient _httpClient;

    public ClienteApiServico(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CadastroResultadoDados> CadastrarAsync(
        ClienteCadastroDados dados,
        CancellationToken cancellationToken = default)
    {
        using var resposta = await _httpClient.PostAsJsonAsync(
            "api/clientes/cadastro",
            dados,
            cancellationToken);

        if (resposta.IsSuccessStatusCode)
        {
            var resultado =
                await resposta.Content.ReadFromJsonAsync<CadastroResultadoDados>(
                    cancellationToken: cancellationToken);

            if (resultado is null)
            {
                throw new InvalidOperationException(
                    "A API retornou uma resposta vazia.");
            }

            return resultado;
        }

        string conteudo = await resposta.Content.ReadAsStringAsync(
            cancellationToken);

        throw new InvalidOperationException(
            LerMensagemErro(conteudo));
    }

    private static string LerMensagemErro(string conteudo)
    {
        const string mensagemPadrao =
            "Não foi possível realizar o cadastro.";

        try
        {
            using var documento = JsonDocument.Parse(conteudo);
            var raiz = documento.RootElement;

            if (raiz.ValueKind != JsonValueKind.Object)
                return mensagemPadrao;

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
                        if (erro.ValueKind == JsonValueKind.String)
                        {
                            string? mensagem = erro.GetString();

                            if (!string.IsNullOrWhiteSpace(mensagem))
                                mensagens.Add(mensagem);
                        }
                    }
                }

                if (mensagens.Count > 0)
                {
                    return string.Join(
                        Environment.NewLine,
                        mensagens.Distinct());
                }
            }

            if (raiz.TryGetProperty("mensagem", out var mensagemApi) &&
                mensagemApi.ValueKind == JsonValueKind.String)
            {
                return mensagemApi.GetString() ?? mensagemPadrao;
            }

            if (raiz.TryGetProperty("title", out var titulo) &&
                titulo.ValueKind == JsonValueKind.String)
            {
                return titulo.GetString() ?? mensagemPadrao;
            }
        }
        catch (JsonException)
        {
            return mensagemPadrao;
        }

        return mensagemPadrao;
    }
}