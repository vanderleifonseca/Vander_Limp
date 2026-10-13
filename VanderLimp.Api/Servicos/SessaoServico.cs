using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using VanderLimp.Api.Dados;

namespace VanderLimp.Api.Servicos;

public class SessaoServico
{
    private readonly IConfiguration _configuracao;

    public SessaoServico(IConfiguration configuracao)
    {
        _configuracao = configuracao;
    }

    public async Task<SessaoDados> CriarAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        string token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        string tokenHash = CalcularHash(token);

        DateTime dataCriacao = DateTime.UtcNow;
        DateTime dataExpiracao = dataCriacao.AddDays(30);

        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO SessoesUsuarios
                (UsuarioId, TokenHash, DataCriacao,
                 DataExpiracao, Revogada)
            VALUES
                (@UsuarioId, @TokenHash, @DataCriacao,
                 @DataExpiracao, 0);
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        comando.Parameters.AddWithValue("@UsuarioId", usuarioId);
        comando.Parameters.AddWithValue("@TokenHash", tokenHash);
        comando.Parameters.AddWithValue("@DataCriacao", dataCriacao);
        comando.Parameters.AddWithValue("@DataExpiracao", dataExpiracao);

        await comando.ExecuteNonQueryAsync(cancellationToken);

        return new SessaoDados
        {
            Token = token,
            DataExpiracao = dataExpiracao
        };
    }

    public async Task<SessaoUsuarioDados?> ValidarAsync(
        string? token,
        CancellationToken cancellationToken = default)
    {
        if (!TokenTemFormatoValido(token))
            return null;

        string tokenHash = CalcularHash(token!);

        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            SELECT
                s.Id AS SessaoId,
                u.Id AS UsuarioId,
                c.Id AS ClienteId,
                c.NomeRazaoSocial,
                s.DataExpiracao
            FROM SessoesUsuarios s
            INNER JOIN Usuarios u ON u.Id = s.UsuarioId
            INNER JOIN Clientes c ON c.UsuarioId = u.Id
            WHERE s.TokenHash = @TokenHash
              AND s.Revogada = 0
              AND s.DataExpiracao > @Agora
              AND u.Ativo = 1
              AND u.TipoAcesso = 'Cliente'
            LIMIT 1;
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        comando.Parameters.AddWithValue("@TokenHash", tokenHash);
        comando.Parameters.AddWithValue("@Agora", DateTime.UtcNow);

        await using var leitor =
            await comando.ExecuteReaderAsync(cancellationToken);

        if (!await leitor.ReadAsync(cancellationToken))
            return null;

        return new SessaoUsuarioDados
        {
            SessaoId = leitor.GetInt64(
                leitor.GetOrdinal("SessaoId")),

            UsuarioId = leitor.GetInt32(
                leitor.GetOrdinal("UsuarioId")),

            ClienteId = leitor.GetInt32(
                leitor.GetOrdinal("ClienteId")),

            Nome = leitor.GetString(
                leitor.GetOrdinal("NomeRazaoSocial")),

            DataExpiracao = DateTime.SpecifyKind(
                leitor.GetDateTime(
                    leitor.GetOrdinal("DataExpiracao")),
                DateTimeKind.Utc)
        };
    }

    public async Task RevogarAsync(
        string? token,
        CancellationToken cancellationToken = default)
    {
        if (!TokenTemFormatoValido(token))
            return;

        string tokenHash = CalcularHash(token!);

        await using var conexao =
            new MySqlConnection(ObterConexao());

        await conexao.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE SessoesUsuarios
            SET Revogada = 1
            WHERE TokenHash = @TokenHash
              AND Revogada = 0;
            """;

        await using var comando = new MySqlCommand(sql, conexao);

        comando.Parameters.AddWithValue("@TokenHash", tokenHash);

        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    public static string CalcularHash(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    private static bool TokenTemFormatoValido(string? token)
    {
        return !string.IsNullOrWhiteSpace(token) &&
               token.Length == 64 &&
               token.All(c =>
                   (c >= '0' && c <= '9') ||
                   (c >= 'A' && c <= 'F'));
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