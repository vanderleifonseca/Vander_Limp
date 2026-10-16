using VanderLimp.Admin.Dados;

namespace VanderLimp.Admin.Servicos;

public class SessaoAdminServico
{
    public string? Token { get; private set; }

    public int UsuarioId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public DateTime DataExpiracao { get; private set; }

    public bool EstaAutenticado =>
        !string.IsNullOrWhiteSpace(Token) &&
        DataExpiracao > DateTime.UtcNow;

    public void Iniciar(LoginAdminResultadoDados dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Token) ||
            dados.UsuarioId <= 0 ||
            dados.DataExpiracao <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "A sessão administrativa é inválida.");
        }

        Token = dados.Token;
        UsuarioId = dados.UsuarioId;
        Email = dados.Email;
        DataExpiracao = dados.DataExpiracao;
    }

    public void Limpar()
    {
        Token = null;
        UsuarioId = 0;
        Email = string.Empty;
        DataExpiracao = default;
    }
}