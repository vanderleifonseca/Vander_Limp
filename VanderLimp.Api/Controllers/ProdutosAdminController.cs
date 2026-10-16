using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using VanderLimp.Api.Dados;
using VanderLimp.Api.Servicos;

namespace VanderLimp.Api.Controllers;

[ApiController]
[Route("api/admin/produtos")]
public class ProdutosAdminController : ControllerBase
{
    private readonly ProdutoServico _produtoServico;
    private readonly SessaoServico _sessaoServico;
    private readonly ILogger<ProdutosAdminController> _logger;

    public ProdutosAdminController(
        ProdutoServico produtoServico,
        SessaoServico sessaoServico,
        ILogger<ProdutosAdminController> logger)
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

        try
        {
            if (!await TemSessaoAdministrativaAsync(cancellationToken))
                return SessaoInvalida();

            var produtos = await _produtoServico.ListarTodosAsync(
                cancellationToken);

            return Ok(produtos);
        }
        catch (MySqlException ex)
        {
            return FalhaBanco(ex);
        }
        catch (InvalidOperationException ex)
        {
            return FalhaConfiguracao(ex);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Cadastrar(
        [FromBody] ProdutoCadastroDados dados,
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";

        try
        {
            if (!await TemSessaoAdministrativaAsync(cancellationToken))
                return SessaoInvalida();

            int id = await _produtoServico.CadastrarAsync(
                dados,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new
            {
                mensagem = "Produto cadastrado com sucesso.",
                id
            });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new
            {
                mensagem = "Já existe um produto com esse código."
            });
        }
        catch (MySqlException ex)
        {
            return FalhaBanco(ex);
        }
        catch (InvalidOperationException ex)
        {
            return FalhaConfiguracao(ex);
        }
    }

    [HttpPut("{id:int:min(1)}")]
    public async Task<IActionResult> Atualizar(
        int id,
        [FromBody] ProdutoCadastroDados dados,
        CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-store";

        try
        {
            if (!await TemSessaoAdministrativaAsync(cancellationToken))
                return SessaoInvalida();

            bool encontrado = await _produtoServico.AtualizarAsync(
                id,
                dados,
                cancellationToken);

            if (!encontrado)
            {
                return NotFound(new
                {
                    mensagem = "Produto não encontrado."
                });
            }

            return Ok(new
            {
                mensagem = "Produto atualizado com sucesso.",
                id
            });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new
            {
                mensagem = "Já existe outro produto com esse código."
            });
        }
        catch (MySqlException ex)
        {
            return FalhaBanco(ex);
        }
        catch (InvalidOperationException ex)
        {
            return FalhaConfiguracao(ex);
        }
    }

    private async Task<bool> TemSessaoAdministrativaAsync(
        CancellationToken cancellationToken)
    {
        if (!AuthenticationHeaderValue.TryParse(
                Request.Headers.Authorization.ToString(),
                out var autorizacao) ||
            !string.Equals(
                autorizacao.Scheme,
                "Bearer",
                StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(autorizacao.Parameter))
        {
            return false;
        }

        var sessao = await _sessaoServico.ValidarAdministradorAsync(
            autorizacao.Parameter,
            cancellationToken);

        return sessao is not null;
    }

    private IActionResult SessaoInvalida()
    {
        return Unauthorized(new
        {
            mensagem = "Entre com uma conta de administrador."
        });
    }

    private IActionResult FalhaBanco(MySqlException ex)
    {
        _logger.LogError(
            ex,
            "Falha na administração de produtos. Código MySQL: {Codigo}",
            ex.Number);

        return Problem(
            title: "Não foi possível realizar a operação com os produtos.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    private IActionResult FalhaConfiguracao(
        InvalidOperationException ex)
    {
        _logger.LogError(
            ex,
            "Falha de configuração na administração de produtos.");

        return Problem(
            title: "Não foi possível realizar a operação com os produtos.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}