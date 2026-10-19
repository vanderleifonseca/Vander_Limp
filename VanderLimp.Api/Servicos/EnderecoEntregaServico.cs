using MySqlConnector;
using VanderLimp.Api.Dados;

namespace VanderLimp.Api.Servicos;

public class EnderecoEntregaServico
{
    private readonly IConfiguration _configuracao;

    public EnderecoEntregaServico(IConfiguration configuracao)
    {
        _configuracao = configuracao;
    }

    public async Task<List<EnderecoEntregaDados>> ListarAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        if (clienteId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(clienteId));
        }

        string? connectionString =
            _configuracao.GetConnectionString("VanderLimp");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A conexão com o banco não está configurada.");
        }

        await using var conexao =
            new MySqlConnection(connectionString);

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            SELECT
                Id,
                Apelido,
                CEP,
                Logradouro,
                Numero,
                SemNumero,
                Complemento,
                Bairro,
                Cidade,
                UF,
                PontoReferencia,
                EhEntregaPadrao
            FROM Enderecos
            WHERE ClienteId = @ClienteId
              AND Ativo = 1
              AND CodigoMunicipioIBGE = '4108304'
              AND UF = 'PR'
            ORDER BY EhEntregaPadrao DESC, Id;
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        comando.Parameters.AddWithValue("@ClienteId", clienteId);

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        var enderecos = new List<EnderecoEntregaDados>();

        while (await leitor.ReadAsync(cancellationToken))
        {
            enderecos.Add(new EnderecoEntregaDados
            {
                Id = leitor.GetInt32(leitor.GetOrdinal("Id")),

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

                PontoReferencia = LerTextoOpcional(
                    leitor,
                    "PontoReferencia"),

                EhEntregaPadrao = leitor.GetBoolean(
                    leitor.GetOrdinal("EhEntregaPadrao"))
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
}