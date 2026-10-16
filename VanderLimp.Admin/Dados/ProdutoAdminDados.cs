namespace VanderLimp.Admin.Dados;

public class ProdutoAdminDados
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string? Marca { get; set; }

    public string UnidadeVenda { get; set; } = "UN";

    public string Conteudo { get; set; } = string.Empty;

    public decimal PrecoUnitario { get; set; }

    public bool Ativo { get; set; }
}