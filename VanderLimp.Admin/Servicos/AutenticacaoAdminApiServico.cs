using System.Net;
using System.Net.Http.Json;
using VanderLimp.Admin.Dados;

namespace VanderLimp.Admin.Servicos;

public class AutenticacaoAdminApiServico
{
    private readonly HttpClient _httpClient;

    public AutenticacaoAdminApiServico(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginAdminResultadoDados> LoginAsync(
        LoginAdminDados dados,
        CancellationToken cancellationToken = default)
    {
        var entrada = new LoginAdminDados
        {
            Email = dados.Email.Trim().ToLowerInvariant(),
            Senha = dados.Senha
        };

        using var resposta = await _httpClient.PostAsJsonAsync(
            "api/admin/autenticacao/login",
            entrada,
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
                "Não foi possível entrar no painel. Tente novamente.");
        }

        var resultado =
            await resposta.Content
                .ReadFromJsonAsync<LoginAdminResultadoDados>(
                    cancellationToken: cancellationToken);

        if (resultado is null ||
            string.IsNullOrWhiteSpace(resultado.Token) ||
            resultado.UsuarioId <= 0 ||
            resultado.DataExpiracao <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "A API retornou uma sessão inválida.");
        }

        return resultado;
    }

    public async Task LogoutAsync(
    string token,
    CancellationToken cancellationToken = default)
    {
        using var requisicao = new HttpRequestMessage(
            HttpMethod.Post,
            "api/autenticacao/logout");

        requisicao.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token);

        using var resposta = await _httpClient.SendAsync(
            requisicao,
            cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.Unauthorized)
            return;

        if (!resposta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "Não foi possível encerrar a sessão. Tente novamente.");
        }
    }
}