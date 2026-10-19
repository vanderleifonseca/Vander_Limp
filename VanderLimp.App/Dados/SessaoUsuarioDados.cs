namespace VanderLimp.App.Dados;

public class SessaoUsuarioDados
{
    public int UsuarioId { get; set; }

    public int ClienteId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public DateTime DataExpiracao { get; set; }
}