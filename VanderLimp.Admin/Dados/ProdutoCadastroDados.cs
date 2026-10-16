using System.ComponentModel.DataAnnotations;

namespace VanderLimp.Admin.Dados;

public class ProdutoCadastroDados : IValidatableObject
{
    [Required(ErrorMessage = "Informe o código do produto.")]
    [StringLength(30)]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a descrição do produto.")]
    [StringLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Marca { get; set; }

    [Required(ErrorMessage = "Informe o conteúdo do produto.")]
    [StringLength(60)]
    public string Conteudo { get; set; } = string.Empty;

    public decimal PrecoUnitario { get; set; }

    public bool Ativo { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (PrecoUnitario <= 0 ||
            PrecoUnitario > 99999999.99m)
        {
            yield return new ValidationResult(
                "Informe um preço maior que zero e de até 99.999.999,99.",
                new[] { nameof(PrecoUnitario) });
        }
        else if (decimal.Round(PrecoUnitario, 2) != PrecoUnitario)
        {
            yield return new ValidationResult(
                "O preço deve ter no máximo duas casas decimais.",
                new[] { nameof(PrecoUnitario) });
        }
    }
}