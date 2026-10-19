using System.ComponentModel.DataAnnotations;

namespace VanderLimp.Api.Dados;

public class PedidoItemCadastroDados
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Informe um produto válido.")]
    public int ProdutoId { get; set; }

    [Range(1, 9999,
        ErrorMessage = "A quantidade deve estar entre 1 e 9999 UN.")]
    public int Quantidade { get; set; }
}