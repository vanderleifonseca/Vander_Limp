using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using VanderLimp.Api.Servicos;

namespace VanderLimp.Api.Controllers;

[ApiController]
[Route("api/enderecos-entrega")]
public class EnderecosEntregaController : ControllerBase
{
    private readonly EnderecoEntregaServico _enderecoServico;
    private readonly SessaoServico _sessaoServico;
    private readonly ILogger<EnderecosEntregaController> _logger;

    public EnderecosEntregaController(
        EnderecoEntregaServico enderecoServico,
        SessaoServico sessaoServico,
        ILogger<EnderecosEntregaController> logger)
    {
        _enderecoServico = enderecoServico;
        _sessaoServico = sessaoServico;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";

        if (!AuthenticationHeaderValue.TryParse(
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
            var sessao = await _sessaoServico.ValidarAsync(
                autorizacao.Parameter,
                cancellationToken);

            if (sessao is null)
            {
                return SessaoInvalida();
            }

            var enderecos = await _enderecoServico.ListarAsync(
                sessao.ClienteId,
                cancellationToken);

            return Ok(enderecos);
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao listar endereços de entrega. " +
                "Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível carregar os endereços de entrega.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Falha de configuração na consulta de endereços de entrega.");

            return Problem(
                title: "Não foi possível carregar os endereços de entrega.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }

    private IActionResult SessaoInvalida()
    {
        return Unauthorized(new
        {
            mensagem = "Sessão inválida ou expirada. Entre novamente."
        });
    }
}