using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using VanderLimp.Api.Servicos;

namespace VanderLimp.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoServico _produtoServico;
    private readonly SessaoServico _sessaoServico;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(
        ProdutoServico produtoServico,
        SessaoServico sessaoServico,
        ILogger<ProdutosController> logger)
    {
        _produtoServico = produtoServico;
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
                return SessaoInvalida();

            var produtos = await _produtoServico.ListarAtivosAsync(
                cancellationToken);

            return Ok(produtos);
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao listar produtos. Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível carregar os produtos.",
                statusCode: 503);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Falha de configuração na consulta de produtos.");

            return Problem(
                title: "Não foi possível carregar os produtos.",
                statusCode: 503);
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