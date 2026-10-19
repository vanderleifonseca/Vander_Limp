using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using VanderLimp.Api.Servicos;

namespace VanderLimp.Api.Controllers;

[ApiController]
[Route("api/admin/clientes")]
public class ClientesAdminController : ControllerBase
{
    private readonly ClienteAdminServico _clienteServico;
    private readonly SessaoServico _sessaoServico;
    private readonly ILogger<ClientesAdminController> _logger;

    public ClientesAdminController(
        ClienteAdminServico clienteServico,
        SessaoServico sessaoServico,
        ILogger<ClientesAdminController> logger)
    {
        _clienteServico = clienteServico;
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
            var sessao =
                await _sessaoServico.ValidarAdministradorAsync(
                    autorizacao.Parameter,
                    cancellationToken);

            if (sessao is null)
            {
                return SessaoInvalida();
            }

            var clientes = await _clienteServico.ListarAsync(
                cancellationToken);

            return Ok(clientes);
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao consultar clientes. Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível consultar os clientes.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Falha de configuração na consulta de clientes.");

            return Problem(
                title: "Não foi possível consultar os clientes.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
    
    [HttpGet("{clienteId:int:min(1)}/enderecos")]
    public async Task<IActionResult> ListarEnderecos(
        int clienteId,
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
            var sessao =
                await _sessaoServico.ValidarAdministradorAsync(
                    autorizacao.Parameter,
                    cancellationToken);

            if (sessao is null)
            {
                return SessaoInvalida();
            }

            var enderecos = await _clienteServico.ListarEnderecosAsync(
                clienteId,
                cancellationToken);

            return Ok(enderecos);
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao consultar endereços do cliente {ClienteId}. " +
                "Código MySQL: {Codigo}",
                clienteId,
                ex.Number);

            return Problem(
                title: "Não foi possível consultar os endereços.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Falha de configuração na consulta de endereços.");

            return Problem(
                title: "Não foi possível consultar os endereços.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
    private IActionResult SessaoInvalida()
    {
        return Unauthorized(new
        {
            mensagem = "Entre com uma conta de administrador."
        });
    }
}