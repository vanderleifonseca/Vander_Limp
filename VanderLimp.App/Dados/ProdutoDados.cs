namespace VanderLimp.App.Dados;

public class ProdutoDados
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string? Marca { get; set; }

    public string UnidadeVenda { get; set; } = "UN";

    public string Conteudo { get; set; } = string.Empty;

    public decimal PrecoUnitario { get; set; }
}