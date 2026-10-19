using MySqlConnector;
using VanderLimp.Api.Dados;

namespace VanderLimp.Api.Servicos;

public class PedidoServico
{
    private readonly IConfiguration _configuracao;

    public PedidoServico(IConfiguration configuracao)
    {
        _configuracao = configuracao;
    }

    public async Task<PedidoResultadoDados?> ConsultarPorChaveAsync(
        int clienteId,
        Guid chaveIdempotencia,
        CancellationToken cancellationToken = default)
    {
        if (clienteId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(clienteId));
        }

        if (chaveIdempotencia == Guid.Empty)
        {
            throw new ArgumentException(
                "Informe uma chave de idempotência válida.",
                nameof(chaveIdempotencia));
        }

        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        return await ConsultarPorChaveNaConexaoAsync(
            conexao,
            null,
            clienteId,
            chaveIdempotencia,
            cancellationToken);
    }

    private static async Task<PedidoResultadoDados?>
        ConsultarPorChaveNaConexaoAsync(
            MySqlConnection conexao,
            MySqlTransaction? transacao,
            int clienteId,
            Guid chaveIdempotencia,
            CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                Id,
                ChaveIdempotencia,
                FormaPagamento,
                StatusPedido,
                StatusPagamento,
                Subtotal,
                TaxaEntrega,
                Total,
                DataCriacao
            FROM Pedidos
            WHERE ClienteId = @ClienteId
              AND ChaveIdempotencia = @ChaveIdempotencia
            LIMIT 1;
            """;

        await using var comando =
            new MySqlCommand(sql, conexao, transacao);

        comando.Parameters.AddWithValue("@ClienteId", clienteId);

        comando.Parameters.AddWithValue(
            "@ChaveIdempotencia",
            chaveIdempotencia.ToString("D"));

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        if (!await leitor.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new PedidoResultadoDados
        {
            PedidoId = leitor.GetInt64(
                leitor.GetOrdinal("Id")),

            ChaveIdempotencia = Guid.Parse(
                leitor.GetString(
                    leitor.GetOrdinal("ChaveIdempotencia"))),

            Mensagem = "Pedido já registrado.",

            FormaPagamento = leitor.GetString(
                leitor.GetOrdinal("FormaPagamento")),

            StatusPedido = leitor.GetString(
                leitor.GetOrdinal("StatusPedido")),

            StatusPagamento = leitor.GetString(
                leitor.GetOrdinal("StatusPagamento")),

            Subtotal = leitor.GetDecimal(
                leitor.GetOrdinal("Subtotal")),

            TaxaEntrega = leitor.GetDecimal(
                leitor.GetOrdinal("TaxaEntrega")),

            Total = leitor.GetDecimal(
                leitor.GetOrdinal("Total")),

            DataCriacao = DateTime.SpecifyKind(
                leitor.GetDateTime(
                    leitor.GetOrdinal("DataCriacao")),
                DateTimeKind.Utc)
        };
    }
    private static async Task ValidarClienteAsync(
    MySqlConnection conexao,
    MySqlTransaction transacao,
    int clienteId,
    CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                u.Ativo,
                c.ComprasBloqueadas
            FROM Clientes c
            INNER JOIN Usuarios u ON u.Id = c.UsuarioId
            WHERE c.Id = @ClienteId
            AND u.TipoAcesso = 'Cliente'
            FOR UPDATE;
            """;

        await using var comando =
            new MySqlCommand(sql, conexao, transacao);

        comando.Parameters.AddWithValue("@ClienteId", clienteId);

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        if (!await leitor.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "O cadastro do cliente não foi encontrado.");
        }

        bool ativo = leitor.GetBoolean(
            leitor.GetOrdinal("Ativo"));

        bool comprasBloqueadas = leitor.GetBoolean(
            leitor.GetOrdinal("ComprasBloqueadas"));

        if (!ativo)
        {
            throw new InvalidOperationException(
                "O cadastro do cliente está inativo.");
        }

        if (comprasBloqueadas)
        {
            throw new InvalidOperationException(
                "As compras deste cadastro estão bloqueadas. " +
                "Entre em contato com a Vander Limp.");
        }
    }

    private static async Task<EnderecoAdminDados> ObterEnderecoAsync(
    MySqlConnection conexao,
    MySqlTransaction transacao,
    int clienteId,
    int enderecoId,
    CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                Id,
                ClienteId,
                CEP,
                Logradouro,
                Numero,
                SemNumero,
                Complemento,
                Bairro,
                Cidade,
                UF,
                CodigoMunicipioIBGE,
                PontoReferencia
            FROM Enderecos
            WHERE Id = @EnderecoId
            AND ClienteId = @ClienteId
            AND Ativo = 1
            AND CodigoMunicipioIBGE = '4108304'
            AND UF = 'PR'
            FOR SHARE;
            """;

        await using var comando =
            new MySqlCommand(sql, conexao, transacao);

        comando.Parameters.AddWithValue("@EnderecoId", enderecoId);
        comando.Parameters.AddWithValue("@ClienteId", clienteId);

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        if (!await leitor.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "O endereço selecionado não está disponível " +
                "para entrega neste cadastro.");
        }

        string? TextoOpcional(string coluna)
        {
            int indice = leitor.GetOrdinal(coluna);

            return leitor.IsDBNull(indice)
                ? null
                : leitor.GetString(indice);
        }

        return new EnderecoAdminDados
        {
            Id = leitor.GetInt32(leitor.GetOrdinal("Id")),

            ClienteId = leitor.GetInt32(
                leitor.GetOrdinal("ClienteId")),

            CEP = leitor.GetString(leitor.GetOrdinal("CEP")),

            Logradouro = leitor.GetString(
                leitor.GetOrdinal("Logradouro")),

            Numero = TextoOpcional("Numero"),

            SemNumero = leitor.GetBoolean(
                leitor.GetOrdinal("SemNumero")),

            Complemento = TextoOpcional("Complemento"),

            Bairro = leitor.GetString(
                leitor.GetOrdinal("Bairro")),

            Cidade = leitor.GetString(
                leitor.GetOrdinal("Cidade")),

            UF = leitor.GetString(leitor.GetOrdinal("UF")),

            CodigoMunicipioIBGE = leitor.GetString(
                leitor.GetOrdinal("CodigoMunicipioIBGE")),

            PontoReferencia = TextoOpcional("PontoReferencia"),

            Ativo = true
        };
    }

    private string ObterConexao()
    {
        string? connectionString =
            _configuracao.GetConnectionString("VanderLimp");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A conexão com o banco não está configurada.");
        }

        return connectionString;
    }
}