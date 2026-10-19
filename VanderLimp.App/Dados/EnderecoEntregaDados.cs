namespace VanderLimp.App.Dados;

public class EnderecoEntregaDados
{
    public int Id { get; set; }

    public string? Apelido { get; set; }

    public string CEP { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public string? Numero { get; set; }

    public bool SemNumero { get; set; }

    public string? Complemento { get; set; }

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string UF { get; set; } = string.Empty;

    public string? PontoReferencia { get; set; }

    public bool EhEntregaPadrao { get; set; }

    public string Resumo
    {
        get
        {
            string numero = SemNumero ? "sem número" : Numero ?? "";

            string apelido = string.IsNullOrWhiteSpace(Apelido)
                ? string.Empty
                : $"{Apelido} — ";

            return $"{apelido}{Logradouro}, {numero} — " +
                   $"{Bairro} — {Cidade}/{UF}";
        }
    }
}