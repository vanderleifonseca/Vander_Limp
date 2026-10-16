using Microsoft.Extensions.DependencyInjection;
using VanderLimp.App.Servicos;

namespace VanderLimp.App.Views;

public partial class PedidosPage : ContentPage, IQueryAttributable
{
    private bool _saindo;

    public PedidosPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(
        IDictionary<string, object> query)
    {
        if (query.TryGetValue("nome", out var valor) &&
            valor is string nome &&
            !string.IsNullOrWhiteSpace(nome))
        {
            MostrarCliente(nome);
        }
    }

    public void MostrarCliente(string nome)
    {
        BoasVindasLabel.Text = $"Bem-vindo, {nome}!";
    }

    private async void OnSairClicked(object? sender, EventArgs e)
    {
        if (_saindo)
            return;

        _saindo = true;
        SairButton.IsEnabled = false;

        bool sessaoRemovida = false;

        try
        {
            var servicos = Handler?.MauiContext?.Services;

            var autenticacao = servicos?
                .GetService<AutenticacaoApiServico>();

            var sessaoLocal = servicos?
                .GetService<SessaoLocalServico>();

            if (autenticacao is null || sessaoLocal is null)
            {
                throw new InvalidOperationException(
                    "O serviço de sessão não está disponível.");
            }

            string? token = await sessaoLocal.ObterTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
            {
                await autenticacao.LogoutAsync(token);
            }

            // Remove o token local após a resposta da API.
            sessaoLocal.EncerrarSessaoLocal();
            sessaoRemovida = true;

            BoasVindasLabel.Text = "Bem-vindo à Vander Limp";

            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (HttpRequestException)
        {
            await DisplayAlertAsync(
                "Não foi possível sair",
                "Confira a conexão com a API e tente novamente. " +
                "A sessão foi mantida no aparelho.",
                "OK");
        }
        catch (OperationCanceledException)
        {
            await DisplayAlertAsync(
                "Tempo de espera esgotado",
                "Tente sair novamente para concluir a operação.",
                "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);

            string mensagem = sessaoRemovida
                ? "A sessão foi removida do aparelho, mas não foi " +
                  "possível abrir o login. Feche e abra o aplicativo."
                : "Não foi possível concluir a saída. Tente novamente.";

            await DisplayAlertAsync(
                "Encerrar sessão",
                mensagem,
                "OK");
        }
        finally
        {
            _saindo = false;
            SairButton.IsEnabled = true;
        }
    }
    private async void OnVerProdutosClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProdutosPage));
    }
}