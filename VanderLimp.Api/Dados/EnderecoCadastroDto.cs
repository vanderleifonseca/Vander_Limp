using System.ComponentModel.DataAnnotations;

namespace VanderLimp.Api.Dados;

public class EnderecoCadastroDto
{
    [Required]
    [RegularExpression(@"^\d{8}$",
        ErrorMessage = "Informe o CEP com 8 números.")]
    public string CEP { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Logradouro { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Numero { get; set; }

    public bool SemNumero { get; set; }

    [StringLength(100)]
    public string? Complemento { get; set; }

    [Required]
    [StringLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Cidade { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[A-Z]{2}$",
        ErrorMessage = "Informe a UF com duas letras maiúsculas.")]
    public string UF { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{7}$",
        ErrorMessage = "Informe o código IBGE com 7 números.")]
    public string CodigoMunicipioIBGE { get; set; } = string.Empty;

    [StringLength(200)]
    public string? PontoReferencia { get; set; }
}