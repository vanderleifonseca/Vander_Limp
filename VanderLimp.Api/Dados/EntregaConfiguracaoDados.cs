using System.ComponentModel.DataAnnotations;

namespace VanderLimp.Api.Dados;

public class EntregaConfiguracaoDados
{
    public const string Secao = "Entrega";

    public bool CobrarTaxa { get; set; }

    public decimal ValorTaxa { get; set; }

    public decimal ObterTaxa()
    {
        if (ValorTaxa < 0 || ValorTaxa > 99999999.99m)
        {
            throw new ValidationException(
                "A taxa de entrega deve estar entre zero e 99.999.999,99.");
        }

        if (decimal.Round(ValorTaxa, 2) != ValorTaxa)
        {
            throw new ValidationException(
                "A taxa de entrega deve ter no máximo duas casas decimais.");
        }

        return CobrarTaxa ? ValorTaxa : 0m;
    }
}