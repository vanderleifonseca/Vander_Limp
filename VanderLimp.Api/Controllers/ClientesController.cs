using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using VanderLimp.Api.Dados;
using VanderLimp.Api.Servicos;

namespace VanderLimp.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IConfiguration _configuracao;
    private readonly SenhaServico _senhaServico;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(
        IConfiguration configuracao,
        SenhaServico senhaServico,
        ILogger<ClientesController> logger)
    {
        _configuracao = configuracao;
        _senhaServico = senhaServico;
        _logger = logger;
    }

    [HttpPost("cadastro")]
    public async Task<IActionResult> Cadastrar(
        [FromBody] ClienteCadastroDto dados,
        CancellationToken cancellationToken)
    {
        var connectionString =
            _configuracao.GetConnectionString("VanderLimp");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Problem(
                title: "O banco de dados não está configurado.",
                statusCode: 503);
        }

        var endereco = dados.Endereco!;

        string email = dados.Email.Trim().ToLowerInvariant();
        string documento = dados.Documento;

        string senhaHash = _senhaServico.GerarHash(dados.Senha);

        // Um endereço cadastral de outra cidade pode ser registrado.
        // Só Foz do Iguaçu poderá ser definida como entrega padrão.
        bool entregaPadrao =
            endereco.CodigoMunicipioIBGE == "4108304" &&
            endereco.UF == "PR" &&
            string.Equals(
                endereco.Cidade.Trim(),
                "Foz do Iguaçu",
                StringComparison.OrdinalIgnoreCase);

        try
        {
            await using var conexao =
                new MySqlConnection(connectionString);

            await conexao.OpenAsync(cancellationToken);

            await using var transacao =
                await conexao.BeginTransactionAsync(cancellationToken);

            // 1. Cadastrar o usuário.
            const string sqlUsuario = """
                INSERT INTO Usuarios
                    (Email, SenhaHash, TipoAcesso, Ativo, EmailConfirmado)
                VALUES
                    (@Email, @SenhaHash, 'Cliente', 1, 0);
                """;

            await using var comandoUsuario =
                new MySqlCommand(sqlUsuario, conexao, transacao);

            comandoUsuario.Parameters.AddWithValue("@Email", email);
            comandoUsuario.Parameters.AddWithValue("@SenhaHash", senhaHash);

            await comandoUsuario.ExecuteNonQueryAsync(cancellationToken);

            long usuarioId = comandoUsuario.LastInsertedId;

            // 2. Cadastrar o cliente.
            const string sqlCliente = """
                INSERT INTO Clientes
                    (UsuarioId, TipoPessoa, NomeRazaoSocial, Documento,
                     NomeFantasia, NomeResponsavel, Telefone)
                VALUES
                    (@UsuarioId, @TipoPessoa, @NomeRazaoSocial, @Documento,
                     @NomeFantasia, @NomeResponsavel, @Telefone);
                """;

            await using var comandoCliente =
                new MySqlCommand(sqlCliente, conexao, transacao);

            comandoCliente.Parameters.AddWithValue("@UsuarioId", usuarioId);
            comandoCliente.Parameters.AddWithValue("@TipoPessoa", dados.TipoPessoa);
            comandoCliente.Parameters.AddWithValue(
                "@NomeRazaoSocial", dados.NomeRazaoSocial.Trim());
            comandoCliente.Parameters.AddWithValue("@Documento", documento);
            comandoCliente.Parameters.AddWithValue(
                "@NomeFantasia", ValorOpcional(dados.NomeFantasia));
            comandoCliente.Parameters.AddWithValue(
                "@NomeResponsavel", ValorOpcional(dados.NomeResponsavel));
            comandoCliente.Parameters.AddWithValue(
                "@Telefone", dados.Telefone.Trim());

            await comandoCliente.ExecuteNonQueryAsync(cancellationToken);

            long clienteId = comandoCliente.LastInsertedId;

            // 3. Cadastrar o endereço.
            const string sqlEndereco = """
                INSERT INTO Enderecos
                    (ClienteId, CEP, Logradouro, Numero, SemNumero,
                     Complemento, Bairro, Cidade, UF, CodigoMunicipioIBGE,
                     PontoReferencia, EhCadastral, EhEntregaPadrao, Ativo)
                VALUES
                    (@ClienteId, @CEP, @Logradouro, @Numero, @SemNumero,
                     @Complemento, @Bairro, @Cidade, @UF, @CodigoMunicipioIBGE,
                     @PontoReferencia, 1, @EhEntregaPadrao, 1);
                """;

            await using var comandoEndereco =
                new MySqlCommand(sqlEndereco, conexao, transacao);

            comandoEndereco.Parameters.AddWithValue("@ClienteId", clienteId);
            comandoEndereco.Parameters.AddWithValue("@CEP", endereco.CEP);
            comandoEndereco.Parameters.AddWithValue(
                "@Logradouro", endereco.Logradouro.Trim());
            comandoEndereco.Parameters.AddWithValue(
                "@Numero",
                endereco.SemNumero
                    ? DBNull.Value
                    : ValorOpcional(endereco.Numero));
            comandoEndereco.Parameters.AddWithValue(
                "@SemNumero", endereco.SemNumero);
            comandoEndereco.Parameters.AddWithValue(
                "@Complemento", ValorOpcional(endereco.Complemento));
            comandoEndereco.Parameters.AddWithValue(
                "@Bairro", endereco.Bairro.Trim());
            comandoEndereco.Parameters.AddWithValue(
                "@Cidade", endereco.Cidade.Trim());
            comandoEndereco.Parameters.AddWithValue("@UF", endereco.UF);
            comandoEndereco.Parameters.AddWithValue(
                "@CodigoMunicipioIBGE", endereco.CodigoMunicipioIBGE);
            comandoEndereco.Parameters.AddWithValue(
                "@PontoReferencia", ValorOpcional(endereco.PontoReferencia));
            comandoEndereco.Parameters.AddWithValue(
                "@EhEntregaPadrao", entregaPadrao);

            await comandoEndereco.ExecuteNonQueryAsync(cancellationToken);

            long enderecoId = comandoEndereco.LastInsertedId;

            await transacao.CommitAsync(cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new
            {
                mensagem = "Cadastro realizado com sucesso.",
                usuarioId,
                clienteId,
                enderecoId
            });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new
            {
                mensagem = "Já existe um cadastro com este e-mail ou documento."
            });
        }
        catch (MySqlException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao cadastrar cliente. Código MySQL: {Codigo}",
                ex.Number);

            return Problem(
                title: "Não foi possível concluir o cadastro.",
                statusCode: 503);
        }
    }

    private static object ValorOpcional(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? DBNull.Value
            : valor.Trim();
    }
}