namespace VanderLimp.App.Dados;

public class EnderecoCadastroDados
{
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
}