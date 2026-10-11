using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace VanderLimp.Api.Controllers;

[ApiController]
[Route("api/teste-banco")]
public class TesteBancoController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TesteBancoController> _logger;

    public TesteBancoController(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<TesteBancoController> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        // Este diagnóstico fica disponível somente em desenvolvimento.
        if (!_environment.IsDevelopment())
            return NotFound();

        var connectionString =
            _configuration.GetConnectionString("VanderLimp");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Problem(
                title: "A conexão com o banco não foi configurada.",
                statusCode: 500);
        }

        try
        {
            await using var connection =
                new MySqlConnection(connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command =
                new MySqlCommand("SELECT DATABASE();", connection);

            var banco = await command.ExecuteScalarAsync(cancellationToken);

            return Ok(new
            {
                mensagem = "Conexão com o MySQL realizada com sucesso.",
                banco
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                "Falha no teste MySQL. Código: {Codigo}", ex.Number);

            return Problem(
                title: "Não foi possível conectar ao MySQL.",
                statusCode: 503);
        }
    }
}