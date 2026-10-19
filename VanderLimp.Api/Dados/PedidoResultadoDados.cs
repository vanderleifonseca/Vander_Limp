namespace VanderLimp.Api.Dados;

public class PedidoResultadoDados
{
    public long PedidoId { get; set; }

    public Guid ChaveIdempotencia { get; set; }

    public string Mensagem { get; set; } = string.Empty;

    public string FormaPagamento { get; set; } = string.Empty;

    public string StatusPedido { get; set; } = string.Empty;

    public string StatusPagamento { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal TaxaEntrega { get; set; }

    public decimal Total { get; set; }

    public DateTime DataCriacao { get; set; }
}