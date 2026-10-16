using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using VanderLimp.Api.Dados;
using VanderLimp.Api.Servicos;

namespace VanderLimp.Api.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly IConfiguration _configuracao;
    private readonly SenhaServico _senhaServico;
    private readonly SessaoServico _sessaoServico;
    private readonly ILogger<AutenticacaoController> _logger;

    public AutenticacaoController(
        IConfiguration configuracao,
        SenhaServico senhaServico,
        SessaoServico sessaoServico,
        ILogger<AutenticacaoController> logger)
    {
        _configuracao = configuracao;
        _senhaServico = senhaServico;
        _sessaoServico = sessaoServico;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDados dados,
        CancellationToken cancellationToken)
    {
        string? connectionString =
            _configuracao.GetConnectionString("VanderLimp");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Problem(
                title: "O banco de dados não está configurado.",
                statusCode: 503);
        }

        try
        {
            int usuarioId;
            int clienteId;
            string nome;
            string senhaHash;
            bool ativo;

            await using (var conexao =
                new MySqlConnection(connectionString))
            {
                await conexao.OpenAsync(cancellationToken);

                const string sql = """
                    SELECT
                        u.Id AS UsuarioId,
                        u.SenhaHash,
                        u.Ativo,
                        c.Id AS ClienteId,
                        c.NomeRazaoSocial
                    FROM Usuarios u
                    INNER JOIN Clientes c ON c.UsuarioId = u.Id
                    WHERE u.Email = @Email
                      AND u.TipoAcesso = 'Cliente'
                    LIMIT 1;
                    """;

                await using var comando =
                    new MySqlCommand(sql, conexao);

                comando.Parameters.AddWithValue(
                    "@Email",
                    dados.Email.Trim().ToLowerInvariant());

                await using var leitor =
                    await comando.ExecuteReaderAsync(cancellationToken);

                if (!await leitor.ReadAsync(cancellationToken))
                {
                    return CredenciaisInvalidas();
                }

                usuarioId = leitor.GetInt32(
                    leitor.GetOrdinal("UsuarioId"));

                clienteId = leitor.GetInt32(
                    leitor.GetOrdinal("ClienteId"));

                nome = leitor.GetString(
                    leitor.GetOrdinal("NomeRazaoSocial"));

                senhaHash = leitor.GetString(
                    leitor.GetOrdinal("SenhaHash"));

                ativo = leitor.GetBoolean(
                    leitor.GetOrdinal("Ativo"));
            }

            bool senhaCorreta = _senhaServico.VerificarSenha(
                senhaHash,
                dados.Senha);

            if (!senhaCorreta || !ativo)
            {
                return CredenciaisInvalidas();
            }

            var sessao = await _sessaoServico.CriarAsync(
                usuarioId,
                cancellationToken);

            return Ok(new
            {
                mensagem = "Login realizado com sucesso.",
                usuarioId,
                clienteId,
                nome,
                token = sessao.Token,
                dataExpiracao = sessao.DataExpiracao
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha no login. Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível realizar o login.",
                statusCode: 503);
        }
    }


    [HttpGet("sessao")]
    public async Task<IActionResult> ConsultarSessao(
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";

        if (!System.Net.Http.Headers.AuthenticationHeaderValue.TryParse(
                Request.Headers.Authorization.ToString(),
                out var autorizacao) ||
            !string.Equals(
                autorizacao.Scheme,
                "Bearer",
                StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(autorizacao.Parameter))
        {
            return Unauthorized(new
            {
                mensagem = "Sessão inválida ou expirada."
            });
        }

        try
        {
            var sessao = await _sessaoServico.ValidarAsync(
                autorizacao.Parameter,
                cancellationToken);

            if (sessao is null)
            {
                return Unauthorized(new
                {
                    mensagem = "Sessão inválida ou expirada."
                });
            }

            return Ok(new
            {
                usuarioId = sessao.UsuarioId,
                clienteId = sessao.ClienteId,
                nome = sessao.Nome,
                dataExpiracao = sessao.DataExpiracao
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao consultar sessão. Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível verificar a sessão.",
                statusCode: 503);
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";

        if (!System.Net.Http.Headers.AuthenticationHeaderValue.TryParse(
                Request.Headers.Authorization.ToString(),
                out var autorizacao) ||
            !string.Equals(
                autorizacao.Scheme,
                "Bearer",
                StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(autorizacao.Parameter))
        {
            return Unauthorized(new
            {
                mensagem = "Informe o token da sessão."
            });
        }

        string token = autorizacao.Parameter;

        if (token.Length != 64 ||
            token.Any(c => !((c >= '0' && c <= '9') ||
                            (c >= 'A' && c <= 'F'))))
        {
            return Unauthorized(new
            {
                mensagem = "Token inválido."
            });
        }

        try
        {
            await _sessaoServico.RevogarAsync(
                token,
                cancellationToken);

            return NoContent();
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao encerrar sessão. Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível encerrar a sessão.",
                statusCode: 503);
        }
    }


    private IActionResult CredenciaisInvalidas()
    {
        return Unauthorized(new
        {
            mensagem = "E-mail ou senha inválidos."
        });
    }
}