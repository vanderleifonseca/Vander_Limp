namespace VanderLimp.Api.Dados;

public class ClienteAdminDados
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string TipoPessoa { get; set; } = string.Empty;

    public string NomeRazaoSocial { get; set; } = string.Empty;

    public string Documento { get; set; } = string.Empty;

    public string? NomeFantasia { get; set; }

    public string? NomeResponsavel { get; set; }

    public string Telefone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    public bool ComprasBloqueadas { get; set; }
}