namespace VanderLimp.Api.Dados;

public class SessaoUsuarioDados
{
    public long SessaoId { get; set; }

    public int UsuarioId { get; set; }

    public int ClienteId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public DateTime DataExpiracao { get; set; }
}