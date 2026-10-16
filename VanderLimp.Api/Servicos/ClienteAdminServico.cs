using MySqlConnector;
using VanderLimp.Api.Dados;

namespace VanderLimp.Api.Servicos;

public class ClienteAdminServico
{
    private readonly IConfiguration _configuracao;

    public ClienteAdminServico(IConfiguration configuracao)
    {
        _configuracao = configuracao;
    }

    public async Task<List<ClienteAdminDados>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            SELECT
                c.Id,
                c.UsuarioId,
                c.TipoPessoa,
                c.NomeRazaoSocial,
                c.Documento,
                c.NomeFantasia,
                c.NomeResponsavel,
                c.Telefone,
                c.ComprasBloqueadas,
                u.Email,
                u.Ativo
            FROM Clientes c
            INNER JOIN Usuarios u ON u.Id = c.UsuarioId
            WHERE u.TipoAcesso = 'Cliente'
            ORDER BY c.NomeRazaoSocial, c.Id;
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        var clientes = new List<ClienteAdminDados>();

        while (await leitor.ReadAsync(cancellationToken))
        {
            clientes.Add(new ClienteAdminDados
            {
                Id = leitor.GetInt32(leitor.GetOrdinal("Id")),

                UsuarioId = leitor.GetInt32(
                    leitor.GetOrdinal("UsuarioId")),

                TipoPessoa = leitor.GetString(
                    leitor.GetOrdinal("TipoPessoa")),

                NomeRazaoSocial = leitor.GetString(
                    leitor.GetOrdinal("NomeRazaoSocial")),

                Documento = leitor.GetString(
                    leitor.GetOrdinal("Documento")),

                NomeFantasia = LerTextoOpcional(
                    leitor,
                    "NomeFantasia"),

                NomeResponsavel = LerTextoOpcional(
                    leitor,
                    "NomeResponsavel"),

                Telefone = leitor.GetString(
                    leitor.GetOrdinal("Telefone")),

                Email = leitor.GetString(
                    leitor.GetOrdinal("Email")),

                Ativo = leitor.GetBoolean(
                    leitor.GetOrdinal("Ativo")),

                ComprasBloqueadas = leitor.GetBoolean(
                    leitor.GetOrdinal("ComprasBloqueadas"))
            });
        }

        return clientes;
    }

    public async Task<List<EnderecoAdminDados>> ListarEnderecosAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        if (clienteId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(clienteId));
        }

        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            SELECT
                Id,
                ClienteId,
                Apelido,
                CEP,
                Logradouro,
                Numero,
                SemNumero,
                Complemento,
                Bairro,
                Cidade,
                UF,
                CodigoMunicipioIBGE,
                PontoReferencia,
                EhCadastral,
                EhEntregaPadrao,
                Ativo
            FROM Enderecos
            WHERE ClienteId = @ClienteId
            ORDER BY Ativo DESC, EhCadastral DESC, Id;
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        comando.Parameters.AddWithValue("@ClienteId", clienteId);

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        var enderecos = new List<EnderecoAdminDados>();

        while (await leitor.ReadAsync(cancellationToken))
        {
            enderecos.Add(new EnderecoAdminDados
            {
                Id = leitor.GetInt32(leitor.GetOrdinal("Id")),

                ClienteId = leitor.GetInt32(
                    leitor.GetOrdinal("ClienteId")),

                Apelido = LerTextoOpcional(leitor, "Apelido"),

                CEP = leitor.GetString(leitor.GetOrdinal("CEP")),

                Logradouro = leitor.GetString(
                    leitor.GetOrdinal("Logradouro")),

                Numero = LerTextoOpcional(leitor, "Numero"),

                SemNumero = leitor.GetBoolean(
                    leitor.GetOrdinal("SemNumero")),

                Complemento = LerTextoOpcional(
                    leitor,
                    "Complemento"),

                Bairro = leitor.GetString(
                    leitor.GetOrdinal("Bairro")),

                Cidade = leitor.GetString(
                    leitor.GetOrdinal("Cidade")),

                UF = leitor.GetString(leitor.GetOrdinal("UF")),

                CodigoMunicipioIBGE = leitor.GetString(
                    leitor.GetOrdinal("CodigoMunicipioIBGE")),

                PontoReferencia = LerTextoOpcional(
                    leitor,
                    "PontoReferencia"),

                EhCadastral = leitor.GetBoolean(
                    leitor.GetOrdinal("EhCadastral")),

                EhEntregaPadrao = leitor.GetBoolean(
                    leitor.GetOrdinal("EhEntregaPadrao")),

                Ativo = leitor.GetBoolean(
                    leitor.GetOrdinal("Ativo"))
            });
        }

        return enderecos;
    }

    private static string? LerTextoOpcional(
        MySqlDataReader leitor,
        string coluna)
    {
        int indice = leitor.GetOrdinal(coluna);

        return leitor.IsDBNull(indice)
            ? null
            : leitor.GetString(indice);
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