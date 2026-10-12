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
        string? connectionString =
            _configuracao.GetConnectionString("VanderLimp");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A conexão com o banco não está configurada.");
        }

        // Token aleatório de 256 bits.
        string token = Convert.ToHexString(
            RandomNumberGenerator.GetBytes(32));

        string tokenHash = CalcularHash(token);

        DateTime dataCriacao = DateTime.UtcNow;
        DateTime dataExpiracao = dataCriacao.AddDays(30);

        await using var conexao =
            new MySqlConnection(connectionString);

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

    public static string CalcularHash(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}