using MySqlConnector;
using VanderLimp.Api.Dados;

namespace VanderLimp.Api.Servicos;

public class ProdutoServico
{
    private readonly IConfiguration _configuracao;

    public ProdutoServico(IConfiguration configuracao)
    {
        _configuracao = configuracao;
    }

    public async Task<List<ProdutoDados>> ListarAtivosAsync(
        CancellationToken cancellationToken = default)
    {
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
                Codigo,
                Descricao,
                Marca,
                UnidadeVenda,
                Conteudo,
                PrecoUnitario
            FROM Produtos
            WHERE Ativo = 1
            ORDER BY Descricao, Id;
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        var produtos = new List<ProdutoDados>();

        while (await leitor.ReadAsync(cancellationToken))
        {
            int colunaMarca = leitor.GetOrdinal("Marca");

            produtos.Add(new ProdutoDados
            {
                Id = leitor.GetInt32(leitor.GetOrdinal("Id")),
                Codigo = leitor.GetString(leitor.GetOrdinal("Codigo")),
                Descricao = leitor.GetString(leitor.GetOrdinal("Descricao")),

                Marca = leitor.IsDBNull(colunaMarca)
                    ? null
                    : leitor.GetString(colunaMarca),

                UnidadeVenda = leitor.GetString(
                    leitor.GetOrdinal("UnidadeVenda")),

                Conteudo = leitor.GetString(
                    leitor.GetOrdinal("Conteudo")),

                PrecoUnitario = leitor.GetDecimal(
                    leitor.GetOrdinal("PrecoUnitario"))
            });
        }

        return produtos;
    }
}