using System.ComponentModel.DataAnnotations;

namespace VanderLimp.Api.Dados;

public class PedidoCadastroDados : IValidatableObject
{
    public Guid ChaveIdempotencia { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Selecione um endereço de entrega.")]
    public int EnderecoId { get; set; }

    [Required(ErrorMessage = "Selecione a forma de pagamento.")]
    [RegularExpression(
        "^(PIX|CARTAO|DINHEIRO)$",
        ErrorMessage = "Forma de pagamento inválida.")]
    public string FormaPagamento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe os produtos do pedido.")]
    [MinLength(1, ErrorMessage = "Selecione pelo menos um produto.")]
    [MaxLength(100, ErrorMessage = "O pedido permite até 100 produtos.")]
    public List<PedidoItemCadastroDados> Itens { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (ChaveIdempotencia == Guid.Empty)
        {
            yield return new ValidationResult(
                "Informe a identificação da tentativa de pedido.",
                new[] { nameof(ChaveIdempotencia) });
        }

        if (Itens is not null)
        {
            if (Itens.Any(item => item is null))
            {
                yield return new ValidationResult(
                    "O pedido contém um item inválido.",
                    new[] { nameof(Itens) });
            }
            else if (Itens
                .GroupBy(item => item.ProdutoId)
                .Any(grupo => grupo.Count() > 1))
            {
                yield return new ValidationResult(
                    "Informe cada produto apenas uma vez, " +
                    "com sua quantidade total.",
                    new[] { nameof(Itens) });
            }
        }
    }
}