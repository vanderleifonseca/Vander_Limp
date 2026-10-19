using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VanderLimp.App.Dados;

public class ProdutoSelecaoDados : INotifyPropertyChanged
{
    private int _quantidade;

    public ProdutoSelecaoDados(ProdutoDados produto)
    {
        Produto = produto;
    }

    public ProdutoDados Produto { get; }

    public int Id => Produto.Id;

    public string Codigo => Produto.Codigo;

    public string Descricao => Produto.Descricao;

    public string? Marca => Produto.Marca;

    public string UnidadeVenda => Produto.UnidadeVenda;

    public string Conteudo => Produto.Conteudo;

    public decimal PrecoUnitario => Produto.PrecoUnitario;

    public int Quantidade
    {
        get => _quantidade;

        set
        {
            if (value < 0 || _quantidade == value)
                return;

            _quantidade = value;

            NotificarAlteracao();
            NotificarAlteracao(nameof(Subtotal));
        }
    }

    public decimal Subtotal => PrecoUnitario * Quantidade;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotificarAlteracao(
        [CallerMemberName] string? propriedade = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propriedade));
    }
}