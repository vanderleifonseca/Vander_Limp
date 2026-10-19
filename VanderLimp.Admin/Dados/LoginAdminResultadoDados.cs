namespace VanderLimp.Admin.Dados;

public class LoginAdminResultadoDados
{
    public string Mensagem { get; set; } = string.Empty;

    public int UsuarioId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime DataExpiracao { get; set; }
}