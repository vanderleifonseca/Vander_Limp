using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using VanderLimp.Api.Dados;
using VanderLimp.Api.Servicos;

namespace VanderLimp.Api.Controllers;

[ApiController]
[Route("api/admin/autenticacao")]
public class AutenticacaoAdminController : ControllerBase
{
    private readonly IConfiguration _configuracao;
    private readonly SenhaServico _senhaServico;
    private readonly SessaoServico _sessaoServico;
    private readonly ILogger<AutenticacaoAdminController> _logger;

    public AutenticacaoAdminController(
        IConfiguration configuracao,
        SenhaServico senhaServico,
        SessaoServico sessaoServico,
        ILogger<AutenticacaoAdminController> logger)
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
        Response.Headers.CacheControl = "no-store";

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
            string email;
            string senhaHash;
            bool ativo;

            await using (var conexao =
                new MySqlConnection(connectionString))
            {
                await conexao.OpenAsync(cancellationToken);

                const string sql = """
                    SELECT Id, Email, SenhaHash, Ativo
                    FROM Usuarios
                    WHERE Email = @Email
                      AND TipoAcesso = 'Administrador'
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
                    leitor.GetOrdinal("Id"));

                email = leitor.GetString(
                    leitor.GetOrdinal("Email"));

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
                mensagem = "Login administrativo realizado com sucesso.",
                usuarioId,
                email,
                token = sessao.Token,
                dataExpiracao = sessao.DataExpiracao
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha no login administrativo. Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível realizar o login administrativo.",
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
            return SessaoInvalida();
        }

        try
        {
            var sessao =
                await _sessaoServico.ValidarAdministradorAsync(
                    autorizacao.Parameter,
                    cancellationToken);

            if (sessao is null)
            {
                return SessaoInvalida();
            }

            return Ok(new
            {
                usuarioId = sessao.UsuarioId,
                email = sessao.Email,
                dataExpiracao = sessao.DataExpiracao
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao consultar sessão administrativa. Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível verificar a sessão administrativa.",
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

    private IActionResult SessaoInvalida()
    {
        return Unauthorized(new
        {
            mensagem = "Sessão administrativa inválida ou expirada."
        });
    }
}