namespace VanderLimp.App.Dados;

public class ClienteCadastroDados
{
    public string TipoPessoa { get; set; } = string.Empty;
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string? NomeResponsavel { get; set; }
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string ConfirmacaoSenha { get; set; } = string.Empty;
    public EnderecoCadastroDados Endereco { get; set; } = new();
}