using System.Net;
using System.Net.Http.Json;
using VanderLimp.App.Dados;

namespace VanderLimp.App.Servicos;

public class AutenticacaoApiServico
{
    private readonly HttpClient _httpClient;

    public AutenticacaoApiServico(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResultadoDados> LoginAsync(
        LoginDados dados,
        CancellationToken cancellationToken = default)
    {
        using var resposta = await _httpClient.PostAsJsonAsync(
            "api/autenticacao/login",
            dados,
            cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException(
                "E-mail ou senha inválidos.");
        }

        if (resposta.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new InvalidOperationException(
                "Confira o e-mail e a senha informados.");
        }

        if (!resposta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "Não foi possível realizar o login. Tente novamente.");
        }

        var resultado =
            await resposta.Content.ReadFromJsonAsync<LoginResultadoDados>(
                cancellationToken: cancellationToken);

        if (resultado is null ||
            string.IsNullOrWhiteSpace(resultado.Token))
        {
            throw new InvalidOperationException(
                "A API retornou uma resposta de login inválida.");
        }

        return resultado;
    }
}