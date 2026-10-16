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
        var produtos = await ConsultarAsync(
            somenteAtivos: true,
            cancellationToken: cancellationToken);

        return produtos.Select(produto => new ProdutoDados
        {
            Id = produto.Id,
            Codigo = produto.Codigo,
            Descricao = produto.Descricao,
            Marca = produto.Marca,
            UnidadeVenda = produto.UnidadeVenda,
            Conteudo = produto.Conteudo,
            PrecoUnitario = produto.PrecoUnitario
        }).ToList();
    }

    public Task<List<ProdutoAdminDados>> ListarTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return ConsultarAsync(
            somenteAtivos: false,
            cancellationToken: cancellationToken);
    }

    public async Task<int> CadastrarAsync(
        ProdutoCadastroDados dados,
        CancellationToken cancellationToken = default)
    {
        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO Produtos
                (Codigo, Descricao, Marca, UnidadeVenda,
                 Conteudo, PrecoUnitario, Ativo)
            VALUES
                (@Codigo, @Descricao, @Marca, 'UN',
                 @Conteudo, @PrecoUnitario, @Ativo);
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        PreencherParametros(comando, dados);

        await comando.ExecuteNonQueryAsync(cancellationToken);

        return checked((int)comando.LastInsertedId);
    }

    public async Task<bool> AtualizarAsync(
        int id,
        ProdutoCadastroDados dados,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return false;

        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE Produtos
            SET Codigo = @Codigo,
                Descricao = @Descricao,
                Marca = @Marca,
                UnidadeVenda = 'UN',
                Conteudo = @Conteudo,
                PrecoUnitario = @PrecoUnitario,
                Ativo = @Ativo
            WHERE Id = @Id;
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        comando.Parameters.AddWithValue("@Id", id);
        PreencherParametros(comando, dados);

        int linhas = await comando.ExecuteNonQueryAsync(
            cancellationToken);

        if (linhas > 0)
            return true;

        // Uma atualização sem mudanças também é válida
        // quando o produto existe.
        await using var verificar = new MySqlCommand(
            "SELECT COUNT(*) FROM Produtos WHERE Id = @Id;",
            conexao);

        verificar.Parameters.AddWithValue("@Id", id);

        return Convert.ToInt64(
            await verificar.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    private async Task<List<ProdutoAdminDados>> ConsultarAsync(
        bool somenteAtivos,
        CancellationToken cancellationToken)
    {
        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            SELECT
                Id,
                Codigo,
                Descricao,
                Marca,
                UnidadeVenda,
                Conteudo,
                PrecoUnitario,
                Ativo
            FROM Produtos
            WHERE (@SomenteAtivos = 0 OR Ativo = 1)
            ORDER BY Descricao, Id;
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        comando.Parameters.AddWithValue(
            "@SomenteAtivos",
            somenteAtivos);

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        var produtos = new List<ProdutoAdminDados>();

        while (await leitor.ReadAsync(cancellationToken))
        {
            int colunaMarca = leitor.GetOrdinal("Marca");

            produtos.Add(new ProdutoAdminDados
            {
                Id = leitor.GetInt32(leitor.GetOrdinal("Id")),

                Codigo = leitor.GetString(
                    leitor.GetOrdinal("Codigo")),

                Descricao = leitor.GetString(
                    leitor.GetOrdinal("Descricao")),

                Marca = leitor.IsDBNull(colunaMarca)
                    ? null
                    : leitor.GetString(colunaMarca),

                UnidadeVenda = leitor.GetString(
                    leitor.GetOrdinal("UnidadeVenda")),

                Conteudo = leitor.GetString(
                    leitor.GetOrdinal("Conteudo")),

                PrecoUnitario = leitor.GetDecimal(
                    leitor.GetOrdinal("PrecoUnitario")),

                Ativo = leitor.GetBoolean(
                    leitor.GetOrdinal("Ativo"))
            });
        }

        return produtos;
    }

    private static void PreencherParametros(
        MySqlCommand comando,
        ProdutoCadastroDados dados)
    {
        comando.Parameters.AddWithValue(
            "@Codigo",
            dados.Codigo.Trim());

        comando.Parameters.AddWithValue(
            "@Descricao",
            dados.Descricao.Trim());

        comando.Parameters.AddWithValue(
            "@Marca",
            string.IsNullOrWhiteSpace(dados.Marca)
                ? (object)DBNull.Value
                : dados.Marca.Trim());

        comando.Parameters.AddWithValue(
            "@Conteudo",
            dados.Conteudo.Trim());

        comando.Parameters.AddWithValue(
            "@PrecoUnitario",
            dados.PrecoUnitario);

        comando.Parameters.AddWithValue(
            "@Ativo",
            dados.Ativo);
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