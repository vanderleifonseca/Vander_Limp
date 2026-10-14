using VanderLimp.App.Views;

namespace VanderLimp.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(ProdutosPage),
            typeof(ProdutosPage));

        Routing.RegisterRoute(
            nameof(RevisarPedidoPage),
            typeof(RevisarPedidoPage));
    }
}