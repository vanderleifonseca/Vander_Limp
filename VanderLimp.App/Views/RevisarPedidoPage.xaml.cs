using System.Collections.ObjectModel;
using System.Globalization;
using VanderLimp.App.Dados;

namespace VanderLimp.App.Views;

public partial class RevisarPedidoPage : ContentPage, IQueryAttributable
{
    private readonly ObservableCollection<ItemPedidoDados> _itens = new();

    public RevisarPedidoPage()
    {
        InitializeComponent();

        ItensCollection.ItemsSource = _itens;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _itens.Clear();

        if (query.TryGetValue("itens", out var valor) &&
            valor is IEnumerable<ItemPedidoDados> itens)
        {
            foreach (var item in itens)
            {
                _itens.Add(item);
            }
        }

        decimal total = _itens.Sum(item => item.Subtotal);

        TotalLabel.Text =
            "Total dos produtos: " +
            total.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));
    }

    private async void OnAlterarProdutosClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}