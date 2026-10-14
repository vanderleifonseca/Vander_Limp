namespace VanderLimp.App.Dados;

public class ItemPedidoDados
{
    public int ProdutoId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string? Marca { get; set; }

    public string Conteudo { get; set; } = string.Empty;

    public string UnidadeVenda { get; set; } = "UN";

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => PrecoUnitario * Quantidade;
}