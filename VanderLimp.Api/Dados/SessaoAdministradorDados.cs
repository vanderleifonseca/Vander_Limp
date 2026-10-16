namespace VanderLimp.Api.Dados;

public class SessaoAdministradorDados
{
    public long SessaoId { get; set; }

    public int UsuarioId { get; set; }

    public string Email { get; set; } = string.Empty;

    public DateTime DataExpiracao { get; set; }
}