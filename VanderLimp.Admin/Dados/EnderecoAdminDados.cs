namespace VanderLimp.Admin.Dados;

public class EnderecoAdminDados
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public string? Apelido { get; set; }

    public string CEP { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public string? Numero { get; set; }

    public bool SemNumero { get; set; }

    public string? Complemento { get; set; }

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string UF { get; set; } = string.Empty;

    public string CodigoMunicipioIBGE { get; set; } = string.Empty;

    public string? PontoReferencia { get; set; }

    public bool EhCadastral { get; set; }

    public bool EhEntregaPadrao { get; set; }

    public bool Ativo { get; set; }
}