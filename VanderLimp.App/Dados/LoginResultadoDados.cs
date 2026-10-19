namespace VanderLimp.App.Dados;

public class LoginResultadoDados
{
    public string Mensagem { get; set; } = string.Empty;

    public int UsuarioId { get; set; }

    public int ClienteId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime DataExpiracao { get; set; }
}