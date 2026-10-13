namespace VanderLimp.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(VanderLimp.App.Views.ProdutosPage),
		typeof(VanderLimp.App.Views.ProdutosPage));
	}
}
