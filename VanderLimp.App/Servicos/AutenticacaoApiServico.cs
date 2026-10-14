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

    public async Task<LoginResultadoDados> LoginAsync(LoginDados dados, CancellationToken cancellationToken = default)
    {
        using var resposta = await _httpClient.PostAsJsonAsync("api/autenticacao/login", dados, cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("E-mail ou senha inválidos.");
        }

        if (resposta.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new InvalidOperationException("Confira o e-mail e a senha informados.");
        }

        if (!resposta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Não foi possível realizar o login. Tente novamente.");
        }

        var resultado =
            await resposta.Content.ReadFromJsonAsync<LoginResultadoDados>(cancellationToken: cancellationToken);

        if (resultado is null || string.IsNullOrWhiteSpace(resultado.Token))
        {
            throw new InvalidOperationException("A API retornou uma resposta de login inválida.");
        }

        return resultado;
    }

    public async Task<SessaoUsuarioDados?> ConsultarSessaoAsync(string token, CancellationToken cancellationToken = default)
    {
        using var requisicao = new HttpRequestMessage(HttpMethod.Get, "api/autenticacao/sessao");

        requisicao.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var resposta = await _httpClient.SendAsync(requisicao, cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.Unauthorized)
        {
            return null;
        }

        if (!resposta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Não foi possível verificar sua sessão. Tente novamente.");
        }

        var sessao =
            await resposta.Content.ReadFromJsonAsync<SessaoUsuarioDados>(cancellationToken: cancellationToken);

        if (sessao is null ||
            sessao.UsuarioId <= 0 || sessao.ClienteId <= 0)
        {
            throw new InvalidOperationException("A API retornou uma resposta de sessão inválida.");
        }

        return sessao;
    }

    public async Task LogoutAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return;

        using var requisicao = new HttpRequestMessage(HttpMethod.Post, "api/autenticacao/logout");

        requisicao.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var resposta = await _httpClient.SendAsync(requisicao, cancellationToken);

        if (resposta.StatusCode == HttpStatusCode.Unauthorized)
        {
            // A API já não aceita essa sessão.
            return;
        }

        if (!resposta.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Não foi possível encerrar a sessão no servidor. " + "Tente novamente.");
        }
    }
}